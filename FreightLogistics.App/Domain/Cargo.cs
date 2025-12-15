namespace FreightLogistics.App.Domain
{
    public class Cargo
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public double WeightTons { get; set; }
        public bool RequiresRefrigeration { get; set; }
    }
}
