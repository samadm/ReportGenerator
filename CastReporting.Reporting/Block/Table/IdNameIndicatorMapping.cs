﻿
/*
 *   Copyright (c) 2016 CAST
 *
 * Licensed under a custom license, Version 1.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License, accessible in the main project
 * source code: Empowerment.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using CastReporting.BLL.Computing;
using CastReporting.Reporting.Atrributes;
using CastReporting.Reporting.Builder.BlockProcessing;
using CastReporting.Reporting.ReportingModel;
using CastReporting.Reporting.Languages;
using CastReporting.Domain;

namespace CastReporting.Reporting.Block.Table
{
    [Block("ID_NAME_INDICATOR_MAPPING")]
    class IdNameIndicatorMapping : TableBlock
    {
        
        #region METHODS
         protected override TableDefinition Content(ReportData reportData, Dictionary<string, string> options)
        {
       
            TableDefinition resultTable = null;
            bool displayShortHeader = (options != null && options.ContainsKey("HEADER") && "SHORT" == options["HEADER"]);
           List<string> rowData = new List<string>();
            rowData.AddRange(new string[] { Labels.Name, Labels.Id });

            if (null != reportData &&
                null != reportData.CurrentSnapshot)
            {
                rowData.AddRange(
                        new[]
                    {

             Constants.BusinessCriteria.TechnicalQualityIndex.ToString(), 
             Constants.BusinessCriteria.TechnicalQualityIndex.GetHashCode().ToString(), 
             Constants.BusinessCriteria.Security.ToString(),
             Constants.BusinessCriteria.Security.GetHashCode().ToString(),
             Constants.BusinessCriteria.Robustness.ToString(), 
             Constants.BusinessCriteria.Robustness.GetHashCode().ToString(), 
             Constants.BusinessCriteria.Performance.ToString(),
             Constants.BusinessCriteria.Performance.GetHashCode().ToString(),
             Constants.BusinessCriteria.Changeability.ToString(),
             Constants.BusinessCriteria.Changeability.GetHashCode().ToString(),
             Constants.BusinessCriteria.Transferability.ToString(),
             Constants.BusinessCriteria.Transferability.GetHashCode().ToString(), 
             Constants.BusinessCriteria.ProgrammingPractices.ToString(),
             Constants.BusinessCriteria.ProgrammingPractices.GetHashCode().ToString(),
             Constants.BusinessCriteria.ArchitecturalDesign.ToString(), 
             Constants.BusinessCriteria.ArchitecturalDesign.GetHashCode().ToString(), 
             Constants.BusinessCriteria.Documentation.ToString(),
             Constants.BusinessCriteria.Documentation.GetHashCode().ToString(),
             Constants.BusinessCriteria.SEIMaintainability.ToString(),
             Constants.BusinessCriteria.SEIMaintainability.GetHashCode().ToString(),
             Constants.QualityDistribution.CostComplexityDistribution.ToString(), 
             Constants.QualityDistribution.CostComplexityDistribution.GetHashCode().ToString(), 
             Constants.QualityDistribution.CyclomaticComplexityDistribution.ToString(), 
             Constants.QualityDistribution.CyclomaticComplexityDistribution.GetHashCode().ToString(), 
             Constants.QualityDistribution.OOComplexityDistribution.ToString(), 
             Constants.QualityDistribution.OOComplexityDistribution.GetHashCode().ToString(), 
             Constants.QualityDistribution.SQLComplexityDistribution.ToString(), 
             Constants.QualityDistribution.SQLComplexityDistribution.GetHashCode().ToString(), 
             Constants.QualityDistribution.CouplingDistribution.ToString(),
             Constants.QualityDistribution.CouplingDistribution.GetHashCode().ToString(),
             Constants.QualityDistribution.ClassFanOutDistribution.ToString(), 
             Constants.QualityDistribution.ClassFanOutDistribution.GetHashCode().ToString(), 
             Constants.QualityDistribution.ClassFanInDistribution.ToString(),
             Constants.QualityDistribution.ClassFanInDistribution.GetHashCode().ToString(),
             Constants.QualityDistribution.SizeDistribution.ToString(), 
             Constants.QualityDistribution.SizeDistribution.GetHashCode().ToString(), 

            });
               

                resultTable = new TableDefinition
                {
                    HasRowHeaders = false,
                    HasColumnHeaders = true,
                    NbRows = 37,
                    NbColumns = 2,
                    Data = rowData
                };
            }
            return resultTable;
        }
        #endregion METHODS
    }
}
