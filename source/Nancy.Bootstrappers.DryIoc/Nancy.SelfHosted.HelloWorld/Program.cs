using DryIoc;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using System;
using Nancy.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;

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

    public abstract class CustomBootstrapper : NancyBootstrapperWithRequestContainerBase<IContainer>
    {
        private List<Type> _moduleTypes;

        protected override IContainer CreateRequestContainer(NancyContext context)
        {
            return ApplicationContainer.OpenScope();
        }

        protected override IEnumerable<INancyModule> GetAllModules(IContainer container)
        {
            return container.Resolve<IEnumerable<INancyModule>>();
        }

        protected override IContainer GetApplicationContainer()
        {
            return new Container(rules =>
                rules.With(FactoryMethod.ConstructorWithResolvableArguments)
            );
        }

        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return ApplicationContainer.Resolve<IEnumerable<IApplicationStartup>>();
        }

        protected override IDiagnostics GetDiagnostics()
        {
            return ApplicationContainer.Resolve<IDiagnostics>();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return ApplicationContainer.Resolve<INancyEngine>();
        }

        protected override INancyModule GetModule(IContainer container, Type moduleType)
        {
            var typeFullName = moduleType.FullName;

            if (container.IsRegistered<INancyModule>(serviceKey: typeFullName))
                return container.Resolve<INancyModule>(serviceKey: typeFullName);

            var instance = (INancyModule)container.New(moduleType);
            container.RegisterInstance(instance, serviceKey: typeFullName);

            return instance;
        }

        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return ApplicationContainer.Resolve<IEnumerable<IRegistrations>>();
        }

        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(IContainer container, Type[] requestStartupTypes)
        {
            foreach (var requestStartupType in requestStartupTypes)
                container.Register(
                    serviceType: typeof(IRequestStartup),
                    implementationType: requestStartupType,
                    reuse: Reuse.Singleton
                );

            return container.Resolve<IEnumerable<IRequestStartup>>();
        }

        protected override void RegisterBootstrapperTypes(IContainer applicationContainer)
        {
            applicationContainer.RegisterInstance<INancyModuleCatalog>(this);
        }

        protected override void RegisterCollectionTypes(IContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrationsn)
        {
            foreach (var collectionTypeRegistration in collectionTypeRegistrationsn)
            {
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    switch (collectionTypeRegistration.Lifetime)
                    {
                        case Lifetime.Transient:
                            container.Register(
                                collectionTypeRegistration.RegistrationType, 
                                implementationType, 
                                reuse: Reuse.Transient
                            );
                            break;
                        case Lifetime.Singleton:
                            container.Register(
                                collectionTypeRegistration.RegistrationType, 
                                implementationType, 
                                reuse: Reuse.Singleton
                            );
                            break;
                        case Lifetime.PerRequest:
                            container.Register(
                                collectionTypeRegistration.RegistrationType,
                                implementationType,
                                reuse: Reuse.InWebRequest
                            );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        protected override void RegisterInstances(IContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
                container.RegisterInstance(instanceRegistration.RegistrationType, instanceRegistration.Implementation);
        }

        protected override void RegisterRequestContainerModules(IContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            _moduleTypes = new List<Type>();

            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                _moduleTypes.Add(moduleRegistrationType.ModuleType);
                container.Register(
                    typeof(INancyModule), 
                    moduleRegistrationType.ModuleType, 
                    serviceKey: moduleRegistrationType.ModuleType.FullName, 
                    ifAlreadyRegistered: IfAlreadyRegistered.Keep
                );
            }
        }

        protected override void RegisterTypes(IContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                switch (typeRegistration.Lifetime)
                {
                    case Lifetime.Transient:
                        container.Register(
                            typeRegistration.RegistrationType, 
                            typeRegistration.ImplementationType, 
                            reuse: Reuse.Transient, 
                            made: FactoryMethod.ConstructorWithResolvableArguments
                        );
                        break;
                    case Lifetime.Singleton:
                        container.Register(
                            typeRegistration.RegistrationType, 
                            typeRegistration.ImplementationType, 
                            reuse: Reuse.Singleton, 
                            made: FactoryMethod.ConstructorWithResolvableArguments
                        );
                        break;
                    case Lifetime.PerRequest:
                        container.Register(
                            typeRegistration.RegistrationType,
                            typeRegistration.ImplementationType,
                            reuse: Reuse.InWebRequest,
                            made: FactoryMethod.ConstructorWithResolvableArguments
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

    public class Bootstrapper : CustomBootstrapper
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
