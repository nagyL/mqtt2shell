using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Mqtt2Shell.Adapters.Windows;

namespace Mqtt2Shell
{
    public class ShellAdapterFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ShellAdapterFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IShellAdapter Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return serviceProvider.GetRequiredService<WindowsShellAdapter>();
            }

            // todo: implement adapters for Linux, Mac etc...
            throw new NotSupportedException();
        }
    }

}
