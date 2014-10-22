﻿//  *************************************************************
// <copyright file="ParamMarkdownNodeParser.cs" company="None">
//     Copyright (c) 2014 andy. All rights reserved.
// </copyright>
// <license>MIT Licence</license>
// <author>andy</author>
// <email>andy.augustin@t-online.de</email>
// *************************************************************

namespace DocToMarkdown
{
    using System;
    using System.Text;
    using System.Xml.Linq;

    using DocToMarkdown.Common;

    /// <summary>
    /// Parameter markdown node parser.
    /// </summary>
    internal class ParamMarkdownNodeParser : IMarkdownNodeParser
    {
        #region fields

        private String _template;
        private IParserPool _parserPool;

        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParamMarkdownNodeParser"/> class.
        /// </summary>
        /// <param name="parserPool">The parser pool.</param>
        /// <param name="environment">The environment.</param>
        internal ParamMarkdownNodeParser(IParserPool parserPool, IEnvironment environment)
        {
            this._parserPool = parserPool;
            this.InitTemplate(environment);
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
            if (element.Name != "param")
            {
                return null;
            }
                
            var elements = element.Elements();
            var stringBuilder = new StringBuilder();

            var description = element.Value;
            stringBuilder.Append(description);

            foreach (var el in elements)
            {
                stringBuilder.Append(this._parserPool.Parse(el));
            }

            var name = element.Attribute("name").Value;

            return String.Format(
                this._template,
                name,
                stringBuilder.ToString());
        }

        #endregion

        #region helper methods

        private void InitTemplate(IEnvironment environment)
        {
            if (!String.IsNullOrEmpty(this._template))
            {
                return;
            }
                
            this._template = String.Format(
                "\tParameter {0}: {1} {2}",
                "{0}",
                "{1}",
                environment.NewLine);
        }

        #endregion
    }
}