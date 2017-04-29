using System;
using System.Threading.Tasks;
using SimpleInjector;
using System.Reflection;

// Simple Command Hanlder Test Project

namespace CommandHandlerTestProject
{
  class CommandResult<TCommand>
  {
    public bool Succeeeded { get; set; }

    public TCommand Command { get; set; }

    public CommandResult(TCommand command, bool succeeded)
    {
      Command = command;
      Succeeeded = succeeded;
    }
  }

  interface ICommandHandlerAsync<TCommand>
  {
    Task<CommandResult<TCommand>> ExecuteAsync(TCommand command);
  }

  class StoopidCommand
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }

  class DummyCommand
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }

  interface ICommandProcessor
  {
    Task<CommandResult<TCommand>> ProcessAsync<TCommand>(TCommand command);
  }

  class CommandProcessor : ICommandProcessor
  {
    private Container _provider;

    public CommandProcessor(Container provider)
    {
      _provider = provider;
    }

    public async Task<CommandResult<TCommand>> ProcessAsync<TCommand>(TCommand command)
    {
      var handlerType = typeof(ICommandHandlerAsync<>).MakeGenericType(command.GetType());

      dynamic handler = _provider.GetInstance(handlerType);

      return await handler.ExecuteAsync((dynamic)command);
    }
  }

  class DummyCommandHandler : ICommandHandlerAsync<DummyCommand>
  {
    public async Task<CommandResult<DummyCommand>> ExecuteAsync(DummyCommand command)
    {
      await Task.Delay(1000);

      command.Id = 2;
      command.Name = "TEst 2";

      return new CommandResult<DummyCommand>(command, true);
    }
  }

  class StoopidCommandHandler : ICommandHandlerAsync<StoopidCommand>
  {
    public async Task<CommandResult<StoopidCommand>> ExecuteAsync(StoopidCommand command)
    {
      await Task.Delay(2000);

      command.Id = 10;
      command.Name = "Stoopid test";

      return new CommandResult<StoopidCommand>(command, true);
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      Test().Wait();
    }

    public async static Task Test()
    {
      Container c = new Container();

      //c.Register<ICommandHandlerAsync<DummyCommand>, DummyCommandHandler>();

      c.Register(typeof(ICommandHandlerAsync<>), new[] { typeof(ICommandHandlerAsync<>).GetTypeInfo().Assembly });

      CommandProcessor cp = new CommandProcessor(c);

      var command = new DummyCommand()
      {
        Id = 1,
        Name = "Name"
      };

      var result = await cp.ProcessAsync(command);

      Console.WriteLine($"{result.Command.Id}");

      var command2 = new StoopidCommand()
      {
        Id = 10,
        Name = "Stoopid"
      };

      var result2 = await cp.ProcessAsync(command2);

      Console.WriteLine($"{result2.Command.Id}");

    }
  }

}