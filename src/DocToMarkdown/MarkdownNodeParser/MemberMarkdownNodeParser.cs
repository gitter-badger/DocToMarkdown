﻿//  *************************************************************
// <copyright file="MemberMarkdownNodeParser.cs" company="None">
//     Copyright (c) 2014 andy. All rights reserved.
// </copyright>
// <license>MIT Licence</license>
// <author>andy</author>
// <email>andy.augustin@t-online.de</email>
// *************************************************************

namespace DocToMarkdown
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using DocToMarkdown.Common;

    /// <summary>
    /// Member markdown node parser.
    /// </summary>
    internal class MemberMarkdownNodeParser : IMarkdownNodeParser
    {
        #region fields

        private readonly Dictionary<String, String> _templateDictionary = new Dictionary<String, String>();
        private readonly IParserPool _parserPool;

        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMarkdownNodeParser"/> class.
        /// </summary>
        /// <param name="parserPool">The parser pool.</param>
        /// <param name="dependencies">The dependency injected parts.</param>
        internal MemberMarkdownNodeParser(IParserPool parserPool, IDependencies dependencies)
        {
            this._parserPool = parserPool;
            this.InitTemplateDictionary(dependencies.Environment);
        }

        #endregion

        #region methods

        /// <summary>
        /// Parses to markdown.
        /// </summary>
        /// <returns>The parsed markdown.</returns>
        /// <param name="element">The element.</param>
        public String ParseToMarkdown(XElement element)
        {
            if (element.Name != "member")
            {
                return null;
            }
                
            var name = element.Attribute("name").Value;
            if (name == null)
            {
                return null;
            }
               
            var memberType = element.Attribute("membertype").Value;

            if (!this._templateDictionary.ContainsKey(memberType))
            {
                return null;
            }

            var template = this._templateDictionary[memberType];

            var elements = element.Elements();
            var stringBuilder = new StringBuilder();

            foreach (var el in elements)
            {
                stringBuilder.Append(this._parserPool.Parse(el));
            }
              
            var nameSpace = element.Attribute("namespace").Value;

            var val = String.Format(
                          "<a name=\"{0}\"></a>{1}",
                          String.Format(
                              "{0}.{1}",
                              nameSpace.ToLower(),
                              name.ToLower()),
                          name);

            return String.Format(
                template,
                val,
                stringBuilder.ToString());
        }

        #endregion

        #region helper methods

        private void InitTemplateDictionary(IEnvironment environment)
        {
            if (this._templateDictionary.Any())
            {
                return;
            }

            // Member is a type
            this._templateDictionary.Add(
                "T",
                String.Format("---{2}#### Type: {0}{2}{2}{1}{2}{2}", "{0}", "{1}", environment.NewLine)); 

            // Member is a method
            this._templateDictionary.Add(
                "M",
                String.Format("#### Method: {0}{2}{2}{2}{1}{2}", "{0}", "{1}", environment.NewLine));

            // Member is a property
            this._templateDictionary.Add(
                "P",
                String.Format("#### Property: {0}{2}{2}{2}{1}{2}", "{0}", "{1}", environment.NewLine));

            // Member is a field
            this._templateDictionary.Add(
                "F",
                String.Format("#### Field: {0}{2}{2}{2}{1}{2}", "{0}", "{1}", environment.NewLine));

            // Member is a event
            this._templateDictionary.Add(
                "E",
                String.Format("#### Event: {0}{2}{2}{2}{1}{2}", "{0}", "{1}", environment.NewLine));
        }

        #endregion
    }
}