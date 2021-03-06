﻿// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class MessageProcessorFactory
    {
        private readonly ISandbox _sandbox;
        private readonly IMethodScanner _stepScanner;
        private Dictionary<Message.Types.MessageType, IMessageProcessor> _messageProcessorsDictionary;
        private readonly ITableFormatter _tableFormatter;
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IActivatorWrapper _activatorWrapper;

        public MessageProcessorFactory(IMethodScanner stepScanner, ISandbox sandbox, IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper, ITableFormatter tableFormatter, IReflectionWrapper reflectionWrapper)
        {
            _tableFormatter = tableFormatter;
            _reflectionWrapper = reflectionWrapper;
            _assemblyLoader = assemblyLoader;
            _activatorWrapper = activatorWrapper;
            _stepScanner = stepScanner;
            _sandbox = sandbox;
            InitializeProcessors(stepScanner);
        }
        public IMessageProcessor GetProcessor(Message.Types.MessageType messageType)
        {
            return _messageProcessorsDictionary.ContainsKey(messageType)
                ? _messageProcessorsDictionary[messageType]
                : new DefaultProcessor();
        }

        private Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(
            IStepRegistry stepRegistry)
        {
            var methodExecutor = new MethodExecutor(_sandbox);
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecutionStarting, new ExecutionStartingProcessor(methodExecutor, _assemblyLoader, _reflectionWrapper)},
                {Message.Types.MessageType.ExecutionEnding, new ExecutionEndingProcessor(methodExecutor, _assemblyLoader, _reflectionWrapper)},
                {
                    Message.Types.MessageType.SpecExecutionStarting,
                    new SpecExecutionStartingProcessor(methodExecutor, _sandbox, _assemblyLoader, _reflectionWrapper)
                },
                {
                    Message.Types.MessageType.SpecExecutionEnding,
                    new SpecExecutionEndingProcessor(methodExecutor, _sandbox, _assemblyLoader, _reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ScenarioExecutionStarting,
                    new ScenarioExecutionStartingProcessor(methodExecutor, _sandbox, _assemblyLoader, _reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ScenarioExecutionEnding,
                    new ScenarioExecutionEndingProcessor(methodExecutor, _sandbox, _assemblyLoader, _reflectionWrapper)
                },
                {Message.Types.MessageType.StepExecutionStarting, new StepExecutionStartingProcessor(methodExecutor, _assemblyLoader, _reflectionWrapper)},
                {Message.Types.MessageType.StepExecutionEnding, new StepExecutionEndingProcessor(methodExecutor, _assemblyLoader, _reflectionWrapper)},
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry, methodExecutor, _tableFormatter)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(_stepScanner)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)},
                {Message.Types.MessageType.ScenarioDataStoreInit, new ScenarioDataStoreInitProcessor(_assemblyLoader)},
                {Message.Types.MessageType.SpecDataStoreInit, new SpecDataStoreInitProcessor(_assemblyLoader)},
                {Message.Types.MessageType.SuiteDataStoreInit, new SuiteDataStoreInitProcessor(_assemblyLoader)},
                {Message.Types.MessageType.StepNameRequest, new StepNameProcessor(stepRegistry)},
                {Message.Types.MessageType.RefactorRequest, new RefactorProcessor(stepRegistry, _sandbox)}
            };
            return messageHandlers;
        }

        private void InitializeProcessors(IMethodScanner stepScanner)
        {
            var stepRegistry = stepScanner.GetStepRegistry();
            _messageProcessorsDictionary = InitializeMessageHandlers(stepRegistry);
        }
    }
}