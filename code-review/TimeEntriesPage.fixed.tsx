// TimeEntriesPage.fixed.tsx
//
// Исправлены:
// 1. useEffect и загрузка данных:
//    - убран бесконечный цикл запросов;
//    - загрузка зависит от props.year и props.month;
//    - добавлена проверка response.ok;
//    - добавлена обработка ошибок.
//
// 2. save:
//    - добавлена проверка ответа сервера;
//    - убрана мутация состояния;
//    - после успешного сохранения список перезагружается с сервера.
//

import React, { useState, useEffect, useCallback } from "react";

interface Props {
    year: number;
    month: number;
}

interface TimeEntryDto {
    id: string;
    date: string;
    employeeId: string;
    employeeName: string;
    projectName: string;
    hours: number;
    amount: number;
}

interface EmployeeDto {
    id: string;
    name: string;
}

export const TimeEntriesPage = (props: Props) => {
    const [entries, setEntries] = useState<TimeEntryDto[]>([]);
    const [employees, setEmployees] = useState<EmployeeDto[]>([]);
    const [employeeId, setEmployeeId] = useState("");
    const [hours, setHours] = useState("");
    const [date, setDate] = useState("");
    const [projectId, setProjectId] = useState("");
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch(
                `/api/time-entries?year=${props.year}&month=${props.month}`
            );

            if (!response.ok) {
                throw new Error("Не удалось загрузить записи табеля.");
            }

            const data = await response.json();
            setEntries(Array.isArray(data) ? data : []);
        } catch {
            setError("Не удалось загрузить записи табеля.");
        } finally {
            setLoading(false);
        }
    }, [props.year, props.month]);

    // FIXED: эффект зависит от года и месяца, а не выполняется после каждого рендера.
    useEffect(() => {
        void load();
    }, [load]);

    useEffect(() => {
        let ignore = false;

        const loadEmployees = async () => {
            try {
                const response = await fetch("/api/employees");

                if (!response.ok) {
                    throw new Error("Не удалось загрузить список сотрудников.");
                }

                const data = await response.json();

                if (!ignore) {
                    setEmployees(Array.isArray(data) ? data : []);
                }
            } catch {
                if (!ignore) {
                    setError("Не удалось загрузить список сотрудников.");
                }
            }
        };

        loadEmployees();

        return () => {
            ignore = true;
        };
    }, []);

    const filtered = employeeId
        ? entries.filter((e) => e.employeeId === employeeId)
        : entries;

    const total = filtered.reduce((sum, entry) => {
        return sum + (Number(entry.amount) || 0);
    }, 0);

    const save = async () => {
        setError(null);
        setSaving(true);

        try {
            const body = {
                employeeId: employeeId,
                projectId: projectId,
                date: new Date(date).toLocaleDateString(),
                hours: hours,
            };

            const response = await fetch("/api/time-entries", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body),
            });

            // FIXED: проверяем ответ сервера.
            if (!response.ok) {
                let message = "Не удалось сохранить запись.";

                try {
                    const errorBody = await response.json();

                    if (errorBody && errorBody.message) {
                        message = errorBody.message;
                    }
                } catch {
                    // Сервер мог вернуть ошибку без тела ответа.
                }

                setError(message);
                return;
            }

            setDate("");
            setProjectId("");
            setHours("");

            // FIXED: не мутируем локальный массив, а перезагружаем список с сервера.
            await load();
        } catch {
            setError("Не удалось сохранить запись.");
        } finally {
            setSaving(false);
        }
    };

    const remove = async (id: string) => {
        setError(null);

        try {
            const response = await fetch(`/api/time-entries/${id}`, {
                method: "DELETE",
            });

            if (!response.ok) {
                setError("Не удалось удалить запись.");
                return;
            }

            await load();
        } catch {
            setError("Не удалось удалить запись.");
        }
    };

    return (
        <div style={{ padding: 20 }}>
            <h2>Табель за {props.month}.{props.year}</h2>

            {error && <div style={{ color: "red" }}>{error}</div>}

            <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
                <option value="">Все сотрудники</option>
                {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                        {emp.name}
                    </option>
                ))}
            </select>

            <div style={{ marginTop: 20 }}>
                <input
                    placeholder="Дата"
                    value={date}
                    onChange={(e) => setDate(e.target.value)}
                />

                <input
                    placeholder="Проект"
                    value={projectId}
                    onChange={(e) => setProjectId(e.target.value)}
                />

                <input
                    placeholder="Часы"
                    value={hours}
                    onChange={(e) => setHours(e.target.value)}
                />

                <button onClick={save} disabled={loading || saving}>
                    Добавить
                </button>
            </div>

            {loading && <div>Загрузка...</div>}

            <table style={{ marginTop: 20, width: "100%" }}>
                <tbody>
                    {filtered.map((entry) => (
                        <tr key={entry.id}>
                            <td>{entry.date}</td>
                            <td>{entry.employeeName}</td>
                            <td>{entry.projectName}</td>
                            <td>{entry.hours}</td>
                            <td>{(Number(entry.amount) || 0).toFixed(2)}</td>
                            <td>
                                <button onClick={() => remove(entry.id)}>
                                    Удалить
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <div style={{ marginTop: 10 }}>
                Итого: {total.toFixed(2)} руб.
            </div>
        </div>
    );
};