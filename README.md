# XakPTSR

VR-система для проведения EMDR- и экспозиционной терапии с удалённым управлением, телеметрией и прямой трансляцией для терапевта.

## Структура репозитория

- `My project1/` — VR/XR-приложение на Unity (`6000.2.12f1`, `Unity.XR.Interaction.Toolkit`).
- `WindowsFormsApp1/` — десктопное приложение-пульт терапевта на C# WinForms (`.NET Framework 4.7.2`).

## Основные компоненты

### 1. VR-клиент (`My project1/`)
- **Управление сессией (`Assets/SessionManager.cs`)** — конечный автомат состояний терапии (`Idle`, `Exposure`, `Pause`, `EMDR`, `SafePlace`, `Finished`), профиль сессии и логирование событий (`SessionLog`, `SessionEvent`, уровень SUD).
- **EMDR-модуль (`Assets/EMDRController.cs`)** — синхронная визуальная, звуковая (левый/правый канал) и тактильная (XR-контроллеры) стимуляция с настраиваемой частотой (`1–4 Hz`).
- **Сцены и инструменты (`Assets/Scripts/`)** — экспозиционные сцены, «безопасное место» (`SafePlaceController.cs`), дыхательные упражнения (`BreathingSquare.cs`, `InteractiveBreathingPanel.cs`), меню паузы и стартовое меню, сетевой обмен и стриминг (`TherapistNetworkLink.cs`, `TherapistRemoteController.cs`, `VRStreamController.cs`).

### 2. Пульт терапевта (`WindowsFormsApp1/`)
- **Управление по UDP (`WindowsFormsApp1/Form1.cs`)** — подключение к VR-устройству (порт по умолчанию `9876`), отправка команд управления сессией.
- **Прямая трансляция** — приём видеопотока из VR-приложения по UDP (порт `9877`).
- **Мониторинг SUD** — контроль уровня субъективного дистресса (`0–10`) и журнал событий.

## Запуск

### Unity VR (`My project1/`)
1. Откройте папку `My project1/` в **Unity Hub** (требуется версия редактора `6000.2.12f1`).
2. Загрузите основную сцену из `Assets/Scenes/` и соберите проект под целевую XR-платформу (например, Meta Quest).

### Пульт терапевта (`WindowsFormsApp1/`)
1. Откройте `WindowsFormsApp1/WindowsFormsApp1.sln` в **Visual Studio**.
2. Соберите и запустите проект (`TargetFrameworkVersion`: `.NET Framework 4.7.2`).
3. Укажите IP-адрес VR-устройства и порт управления (по умолчанию `127.0.0.1:9876`).
