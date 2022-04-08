namespace BancDelTemps.ApiRest.Models
{
    public class CountDTO
    {
        public CountDTO(int total) => Count = total;
        public int Count { get; set; }
    }
    public class IsOkDTO
    {
        public IsOkDTO(bool isOk) => IsOk = isOk;
        public bool IsOk { get; set; }
    }
    public class ParamDTO
    {
        public ParamDTO(string param) => Param = param;
        public string Param { get; set; }
    }

}
