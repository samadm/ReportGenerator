﻿/*
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
using CastReporting.Reporting.Atrributes;
using CastReporting.Reporting.Builder.BlockProcessing;
using CastReporting.Reporting.ReportingModel;
using CastReporting.Reporting.Languages;
using CastReporting.BLL.Computing;
using CastReporting.Domain;

namespace CastReporting.Reporting.Block.Table
{
    [Block("TC_IMPROVEMENT_OPPORTUNITY")]
    class TCImprovementOpportunity : TableBlock
    {
        protected override TableDefinition Content(ReportData reportData, Dictionary<string, string> options)
        {
            Int32 rowCount = 0;
            List<string> rowData = new List<string>();
            rowData.AddRange(new string[] { Labels.TechnicalCriterionName,  Labels.ViolationsCount, Labels.TotalChecks, Labels.Grade });

            #region Options
            
            int nbLimitTop = 0;
            if (null == options || !options.ContainsKey("COUNT") || !Int32.TryParse(options["COUNT"], out nbLimitTop))
            {
                nbLimitTop = reportData.Parameter.NbResultDefault;
            }
            
            int bcCriteriaId = 0;
            if (null == options || !options.ContainsKey("PAR") || !Int32.TryParse(options["PAR"], out bcCriteriaId))
            {
                throw new ArgumentException("Impossible to build RC_IMPROVEMENT_OPPORTUNITY : Need business criterion id.");
            }
            #endregion Options


            var technicalCriticalViolation = RulesViolationUtility.GetTechnicalCriteriaViolations(reportData.CurrentSnapshot,
                                                                                                     (Constants.BusinessCriteria)bcCriteriaId,
                                                                                                     nbLimitTop);
            if(technicalCriticalViolation!=null)
            {
                foreach (var item in technicalCriticalViolation)
                {
                    rowData.AddRange(new string[] 
                                    { 
                                          item.Name
                                        , item.TotalFailed.HasValue ? item.TotalFailed.Value.ToString("N0") : Constants.No_Value
                                        , item.TotalChecks.HasValue ? item.TotalChecks.Value.ToString("N0") : Constants.No_Value                                        
                                        , item.Grade.HasValue ? item.Grade.Value.ToString("N2") : Constants.No_Value
                                   }
                                   );
                }

                rowCount = technicalCriticalViolation.Count;
            }

            TableDefinition resultTable = new TableDefinition
            {
                HasRowHeaders = false,
                HasColumnHeaders = true,
                NbRows = rowCount + 1,
                NbColumns = 4,
                Data = rowData
            };
            return resultTable;
        }
    }
}
