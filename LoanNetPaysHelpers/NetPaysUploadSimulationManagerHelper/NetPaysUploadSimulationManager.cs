using System.Data;

namespace LapoLoanWebApi.LoanNetPaysHelpers.NetPaysUploadSimulationManagerHelper
{
    public class NetPaysUploadSimulationManager
    {
        public List<Task<Task>> Simulations = new List<Task<Task>>();

        public void AddSimulation()
        {

            Simulations.Add(new Task<Task>(async () => await new NetPaysUploadSimulation().Simulate()));
        }

        public async Task StartSimulations(DataTable dataTable1, DataTable dataTable2, DataTable dataTable3)
        {
            var TotalDataUpload = (long)(dataTable1.Rows.Count + dataTable2.Rows.Count + dataTable3.Rows.Count);

            for (long i = 0; i <= TotalDataUpload; i++)
            {
                AddSimulation();
            }

            Simulations.ForEach(s => s.Start());
            await Task.WhenAll(Simulations.Select(x => x.Unwrap()).ToArray());

            Console.WriteLine("All tasks finished");
        }

        //public List<Func<Task>> Simulations1 = new List<Func<Task>>();

        //public void AddSimulation(SimulationParameters parameters)
        //{
        //    Simulations1.Add(() => new NetPaysUploadSimulation().Simulate());
        //}

        //public async Task StartSimulations()
        //{
        //    var tasks = Simulations.Where(s=>s.);
        //    await Task.WhenAll(tasks);

        //    Console.WriteLine("All tasks finished");
        //}
    }

    public class NetPaysUploadSimulation
    {
        public async Task Simulate()
        {
            Console.WriteLine("Simulating");
            await Task.Delay(1000);
            Console.WriteLine("Finished Simulating");
        }
    }
}
