# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some Oxlint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## Запуск и сборка

```bash
npm install        # установка зависимостей
npm run dev        # dev-сервер с HMR
npm run build      # production-сборка (tsc + vite build)
npm run preview    # предпросмотр production-сборки
npm run lint       # проверка oxlint
```

## Переменные окружения

Скопируйте `.env.example` в `.env` и укажите базовый URL API:

```bash
cp .env.example .env
```

| Переменная | Описание | Пример |
|---|---|---|
| `VITE_API_BASE_URL` | Базовый URL backend API | `http://localhost:5000` |

Без `.env`-файла приложение работает на mock-репозиториях. Переменная потребуется при подключении HTTP-клиента.

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the Oxlint configuration

If you are developing a production application, we recommend enabling type-aware lint rules by installing `oxlint-tsgolint` and editing `.oxlintrc.json`:

```json
{
  "$schema": "./node_modules/oxlint/configuration_schema.json",
  "plugins": ["react", "typescript", "oxc"],
  "options": {
    "typeAware": true
  },
  "rules": {
    "react/rules-of-hooks": "error",
    "react/only-export-components": ["warn", { "allowConstantExport": true }]
  }
}
```

See the [Oxlint rules documentation](https://oxc.rs/docs/guide/usage/linter/rules) for the full list of rules and categories.
