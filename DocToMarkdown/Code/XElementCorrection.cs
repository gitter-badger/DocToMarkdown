﻿//  *************************************************************
// <copyright file="XElementCorrection.cs" company="None">
//     Copyright (c) 2014 andy.  All rights reserved.
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
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using DocToMarkdown.Common;

    /// <summary>
    /// Correction for the cryptic member name in the xml.
    /// </summary>
    internal sealed class XElementCorrection
    {
        #region fields

        private readonly Regex _memberTypeRegex = new Regex(@"^.*?(?=:)");
        private Dictionary<String, List<XElement>> _namespaceDictionary = new Dictionary<String, List<XElement>>();
        private ILogger _logger;

        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocToMarkdown.XElementCorrection"/> class.
        /// </summary>
        /// <param name="loggermanager">The logger manager.</param>
        internal XElementCorrection(ILoggerManager loggermanager)
        {
            this._logger = loggermanager.GetLogger("XElementCorrection");
        }

        #endregion

        #region methods

        /// <summary>
        /// Corrections the and namespace order X element.
        /// </summary>
        /// <returns>The and namespace order X element.</returns>
        /// <param name="element">The element.</param>
        internal Dictionary<String, XElement> CorrectionAndNamespaceOrderXElement(XElement element)
        {
            this._logger.Debug("---- Started XElement correction.");

            var members = element.Descendants("member");

            this._logger.Debug("-- Started member correction.");
            foreach (var member in members)
            {
                this.MemberCorrection(member);
            }

            this._logger.Debug("-- Finished member correction.");

            var assemblyElement = element.Element("assembly");

            if (assemblyElement == null)
            {
                const String Message = "assembly element not found";

                this._logger.Info(Message);
            }

            var dict = new Dictionary<String, XElement>();

            this._logger.Debug("-- Started namespace ordering.");

            foreach (var nameSpace in this._namespaceDictionary.Keys)
            {
                var membersElement = new XElement("members");

                membersElement.Add(this._namespaceDictionary[nameSpace]);

                var docElement = new XElement("doc", assemblyElement, membersElement);

                dict.Add(nameSpace, docElement);
            }

            this._logger.Debug("-- Finished member correction.");

            this._logger.Debug("---- Finished XElement correction.");

            return dict;
        }

        #endregion

        #region helper methods

        private void MemberCorrection(XElement element)
        {
            var completeName = element.Attribute("name").Value;

            // Check if element is for method, class, ...
            var memberType = this._memberTypeRegex.Match(completeName).Value;

            element.SetAttributeValue("membertype", memberType);

            completeName = Regex.Replace(completeName, String.Format("{0}:", memberType), String.Empty);

            // Get the name
            var valueWithoutParenthesis = Regex.Replace(completeName, "\\([^\\(]*\\)", String.Empty);

            var splitMatch = Regex.Split(valueWithoutParenthesis, "\\.").ToList();
            var name = splitMatch.Last();
            splitMatch.Remove(name);
             
            String nameSpace = null;

            if (memberType == "T")
            {
                nameSpace = String.Join(".", splitMatch);
            }
            else
            {
                splitMatch.Remove(splitMatch.Last());
                nameSpace = String.Join(".", splitMatch);
            }
                
            // Check for type parameter
            var typeParameterSplit = Regex.Split(name, @"`");

            var fullNameSpaceAddition = typeParameterSplit.FirstOrDefault();

            if (memberType == "T")
            {
                var temp = String.Format(
                               "{0}:{1}.{2}",
                               memberType,
                               nameSpace,
                               fullNameSpaceAddition);

                element.SetAttributeValue("namespace", temp);
            }

            if (typeParameterSplit.Count() > 1)
            {
                name = String.Format("{0}|{1}|", typeParameterSplit.First(), "{0}");

                var typeParameters = element.Elements("typeparam").Attributes("name").Select(tp => tp.Value).ToList();

                if (!typeParameters.Any())
                {
                    name = String.Format(name, @"?");
                }

                var count = 1;
                foreach (var typeParameter in typeParameters)
                {
                    if (count != typeParameters.Count)
                    {
                        name = String.Format(name, String.Format("{0}, {1}", typeParameter, "{0}"));
                    }
                    else
                    {
                        name = String.Format(name, typeParameter);
                    }

                    count++;
                }
            }

            element.SetAttributeValue("name", name);

            if (!this._namespaceDictionary.ContainsKey(nameSpace))
            {
                this._namespaceDictionary.Add(nameSpace, new List<XElement>());
            }

            var seeElements = element.Descendants("see").ToList();

            for (var i = 0; i < seeElements.Count(); i++)
            {
                var seeElement = seeElements[i];
                this.SeeCorrection(ref seeElement);
            }

            this._namespaceDictionary[nameSpace].Add(element);
        }

        private void SeeCorrection(ref XElement seeElement)
        {
            var classRef = seeElement.Attribute("cref");
            if (classRef == null)
            {
                return;
            }

            seeElement.SetAttributeValue("cref", Regex.Split(classRef.Value, @"`").FirstOrDefault());
        }

        #endregion
    }
}