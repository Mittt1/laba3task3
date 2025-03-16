open System
open System.IO

// Функция для получения корректного пути к каталогу от пользователя
let rec getdirectory () : string =
    printf "Введите путь к каталогу: "
    let inputPath = Console.ReadLine()

    if String.IsNullOrWhiteSpace(inputPath) then
        printfn "Ошибка: Путь к каталогу не может быть пустым."
        getdirectory ()
    elif not (Directory.Exists(inputPath)) then
        printfn "Ошибка: Каталог '%s' не найден." inputPath
        getdirectory ()
    else
        inputPath

// Функция для получения начального символа (или строки) от пользователя
let rec getfirstvalue () : string =
    printf "Введите 1-ый символ: "
    let inputStart = Console.ReadLine()

    if String.IsNullOrEmpty(inputStart) then
        printfn "Ошибка: 1-ый символ не может быть пустым."
        getfirstvalue ()
    else
        inputStart

// Функция для подсчета файлов, начинающихся с заданной строки, с демонстрацией отложенных вычислений
let countfiles (directoryPath: string) (startString: string) : Async<Result<int, exn>> =
    async {
        try
            // Directory.EnumerateFiles возвращает ленивую последовательность полных путей к файлам в каталоге.
            let filesSequenceLazy = lazy (Directory.EnumerateFiles(directoryPath))

            // Демонстрация: на данный момент файлы из каталога еще не были прочитаны.
            printfn "\nД: Последовательность файлов для каталога '%s' создана, но не вычислена" directoryPath

            // Фильтрация последовательности. 
            let filteredFiles =
                filesSequenceLazy.Value
                |> Seq.filter (fun filePath ->
                    let fileName = Path.GetFileName(filePath)
                    // Используем StartsWith с IgnoreCase для регистронезависимого сравнения
                    fileName.StartsWith(startString, StringComparison.OrdinalIgnoreCase)
                )

            printfn "Д: Фильтрация файлов при подсчете"

            // Подсчет количества элементов в отфильтрованной последовательности.
            let count = Seq.length filteredFiles

            return Ok count
        with
        | :? UnauthorizedAccessException as ex ->
            return Error (Exception($"Ошибка доступа к каталогу '{directoryPath}'.", ex))
        | ex ->
            return Error ex
    }

[<EntryPoint>]
let main argv =
    printfn "Подсчет файлов по 1-му символу"

    let directoryPath = getdirectory ()
    let startString = getfirstvalue ()

    // Асинхронный блок для работы с асинхронной функцией countFilesStartingWith
    async {
        let! countResult = countfiles directoryPath startString

        match countResult with
        | Ok count ->
            printfn "\nКоличество файлов, начинающихся с '%s': %d" startString count
            return 0 // Успешное завершение
        | Error ex ->
            printfn "\nПроизошла ошибка при подсчете файлов:"
            printfn "Сообщение: %s" ex.Message
            return 1 // Код ошибки
    }
    |> Async.RunSynchronously
