import * as Yup from 'yup';

export interface TimeEntryFormValues {
  employeeId: string;
  projectId: string;
  date: string;
  hours: string;
  comment: string;
}

export const timeEntryValidationSchema = Yup.object({
  employeeId: Yup.string().required('Выберите сотрудника'),
  projectId: Yup.string().required('Выберите проект'),
  date: Yup.string()
    .required('Укажите дату')
    .matches(/^\d{4}-\d{2}-\d{2}$/, 'Дата в формате ГГГГ-ММ-ДД'),
  hours: Yup.string()
    .required('Укажите часы')
    .test('valid-hours', 'Часы должны быть >0, кратны 0.5 и не более 24', (val) => {
      if (!val) return false;
      const normalized = val.replace(',', '.');
      const num = Number(normalized);
      if (isNaN(num)) return false;
      if (num <= 0 || num > 24) return false;
      return (num * 10) % 5 === 0;
    }),
  comment: Yup.string().max(500, 'Не более 500 символов'),
});

/** Нормализует десятичную запятую в точку */
export function normalizeHoursInput(value: string): string {
  return value.replace(',', '.');
}
