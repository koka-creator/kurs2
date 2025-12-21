# Инструкция по созданию GitHub Release

## Как создать Release вручную:

1. Перейдите на https://github.com/koka-creator/kurs2
2. Нажмите на "Releases" справа (или перейдите по ссылке https://github.com/koka-creator/kurs2/releases)
3. Нажмите "Create a new release"
4. Заполните:
   - **Tag version**: `v1.0`
   - **Release title**: `Release v1.0 - Готовое приложение`
   - **Description**: 
     ```
     Готовое приложение для курсовой работы.
     
     ## Как использовать:
     1. Скачайте FreightLogistics.App.zip
     2. Распакуйте архив
     3. Запустите FreightLogistics.App.exe
     
     ## Что включено:
     - Исполняемый файл (не требует установки .NET)
     - База данных с 10 грузовиками, 10 водителями и 10 рейсами
     ```
5. В разделе "Attach binaries" нажмите "Choose your files" и выберите файл `FreightLogistics.App.zip` из **корня репозитория** (не из папки FreightLogistics.App)
6. Нажмите "Publish release"

Готово! Теперь пользователи смогут скачать готовое приложение прямо из раздела Releases.

