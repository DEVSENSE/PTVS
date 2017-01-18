// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.PythonTools.TestAdapter {
    [FileExtension(".py")]
    [DefaultExecutorUri(PythonConstants.TestExecutorUriString)]
    class TestDiscoverer : ITestDiscoverer {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
            ValidateArg.NotNull(sources, "sources");
            ValidateArg.NotNull(discoverySink, "discoverySink");

            var settings = discoveryContext.RunSettings;
            
            DiscoverTests(sources, logger, discoverySink, settings);
        }

        static void DiscoverTests(IEnumerable<string> sources, IMessageLogger logger, ITestCaseDiscoverySink discoverySink, IRunSettings settings) {
            var sourcesSet = new HashSet<string>(sources, StringComparer.OrdinalIgnoreCase);

            var executorUri = new Uri(PythonConstants.TestExecutorUriString);
            // Test list is sent to us via our run settings which we use to smuggle the
            // data we have in our analysis process.
            var doc = new XPathDocument(new StringReader(settings.SettingsXml));
            foreach (var t in TestReader.ReadTests(doc, sourcesSet, m => {
                logger?.SendMessage(TestMessageLevel.Warning, m);
            })) {
                var tc = new TestCase(t.FullyQualifiedName, executorUri, t.SourceFile) {
                    DisplayName = t.DisplayName,
                    LineNumber = t.LineNo,
                    CodeFilePath = t.FileName
                };

                discoverySink.SendTestCase(tc);
            }
        }
    }
}
