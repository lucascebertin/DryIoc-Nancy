using DryIoc;
using Nancy.Hosting.Self;
using System;
using System.Diagnostics;
using Nancy.Bootstrappers.DryIoc;

namespace Nancy.SelfHosted.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new NancyHost(new Bootstrapper(), new Uri("http://localhost:1234")))
            {
                host.Start();
                Process.Start("http://localhost:1234/home");
                Console.WriteLine("The server is listening on: localhost:1234");
                Console.ReadLine();
            }
        }
    }

    public interface ITest
    {
        void Test123();
    }

    public class Test : ITest
    {
        public void Test123()
        {
            
        }
    }

    public class Bootstrapper : DryIocNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(IContainer existingContainer)
        {
            existingContainer.Register<ITest, Test>(Reuse.Transient);
            base.ConfigureApplicationContainer(existingContainer);
        }

        protected override void ConfigureRequestContainer(IContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
        }
    }
}
