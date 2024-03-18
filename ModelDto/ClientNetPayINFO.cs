namespace LapoLoanWebApi.ModelDto
{
    public class ClientNetPayINFO
    {
        public string Bank_Name { get; set; }
        public string Account_Number { get; set; }
        public DateTime NPFDate { get; set; }
        public string Command { get; set; }
        public string Grade_Step { get; set; }

        public string Pay_Group { get; set; }
        public string Grade { get; set; }
        public double Net_Pay { get; set; }
        public string Full_Name { get; set; }
        public string Staff_Id { get; set; }
    }

    public class ClientNetPayINFOEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public DateTime NPFDate { get; set; }
        public int Code { get; set; }
    }
}
