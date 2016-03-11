using System.Diagnostics;
using System.Threading.Tasks;

namespace Nancy.SelfHosted.HelloWorld.Modules
{
    public class HomeModule : NancyModule
    {
        private readonly ITransient _transient;
        private readonly IRequestScoped _requestScoped;

        public HomeModule(ITransient transient, IRequestScoped requestScoped) : base("/home")
        {
            _transient = transient;
            _requestScoped = requestScoped;
            Debug.Assert(_requestScoped == _transient.RequestScoped);

            Get["/"] = _ =>
            {
                var viewBag = new DynamicDictionary();
                viewBag.Add("Transient", _transient);
                viewBag.Add("RequestScoped", _requestScoped);
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