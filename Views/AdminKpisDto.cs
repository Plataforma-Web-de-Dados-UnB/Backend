namespace api.Views
{
    public class AdminKpisDto
    {
        public int TotalPipelines { get; set; }
        public int TotalCategorias { get; set; }
        public int TotalPaineis { get; set; }
        public long VolumeBronze { get; set; }
        public long VolumeSilver { get; set; }
        public long VolumeGold { get; set; }
        public long LinhasBronze { get; set; }
        public long LinhasSilver { get; set; }
        public long LinhasGold { get; set; }
        public bool DatabaseOnline { get; set; }
        public bool SupersetOnline { get; set; }
        public bool RedisOnline { get; set; }
        public int TotalSugestoesPendentes { get; set; }
        public int TotalUsuariosPendentes { get; set; }
    }
}
