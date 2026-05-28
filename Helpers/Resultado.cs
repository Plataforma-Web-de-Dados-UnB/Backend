namespace api.Helpers
{
    public class Resultado<T>
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public T? Data { get; init; }

        public static Resultado<T> Ok(T data) => new() { Success = true, Data = data };
        public static Resultado<T> Falha(string error) => new() { Success = false, Error = error };
    }

    public class ResultadoPaginado<T>
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public int Page { get; init; }
        public int Limit { get; init; }
        public int TotalItens { get; init; }
        public int TotalPaginas { get; init; }
        public List<T> Itens { get; init; } = [];

        public static ResultadoPaginado<T> Ok(int page, int limit, int totalItens, List<T> itens) =>
            new()
            {
                Success = true,
                Page = page,
                Limit = limit,
                TotalItens = totalItens,
                TotalPaginas = (int)Math.Ceiling((double)totalItens / limit),
                Itens = itens
            };

        public static ResultadoPaginado<T> Falha(string error) => new() { Success = false, Error = error };
    }
}
