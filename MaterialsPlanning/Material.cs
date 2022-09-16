namespace MaterialsPlanning
{
    class Material
    {
        public int Id { get; set; }
        public string SapNum { get; set; }
        public string Name { get; set; }
        public string? ProductNum { get; set; } = null;

        public string Producer { get; set; }
        public string Measure { get; set; }

        public int QtyOnStock { get; set; }
        public int QtyOrdered { get; set; }
        public int QtySpentForPeriod { get; set; }
        public int PeriodForAnalysis { get; set; }
        public decimal PriceStockSupply { get; set; }
        public decimal PriceAverageForPeriod { get; set; }
        public decimal PriceLastContract { get; set; }
        public decimal PricePlanned { get; set; }

        public string LastContract { get; set; }
        public string LastSupplier { get; set; }

        public DateTime lastContractDate { get; set; }
    }
}