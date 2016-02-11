using System.Threading.Tasks;

namespace Nancy.SelfHosted.HelloWorld.Modules
{
    public class HomeModule : NancyModule
    {
        private readonly ITest _testInjected;

        public HomeModule(ITest test) : base("/home")
        {
            _testInjected = test;

            Get["/"] = _ =>
            {
                var viewBag = new DynamicDictionary();
                viewBag.Add("test", 1);
                return View["home/index", viewBag];
            };

            Get["/index", runAsync: true] = async (_, token) =>
            {
                await Task.Delay(1000);
                return "123";
            };

            Get["/list", runAsync: true] = async (_, token) =>
            {
                await Task.Delay(1);
                return 500;
            };

            Get["/edit", runAsync: true] = async (_, token) =>
            {
                await Task.Delay(1);
                return 404;
            };
        }
    }
}