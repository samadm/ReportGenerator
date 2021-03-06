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
using CastReporting.Domain;
using CastReporting.Reporting.Atrributes;
using CastReporting.Reporting.Builder.BlockProcessing;
using CastReporting.Reporting.ReportingModel;
using CastReporting.Reporting.Languages;
using CastReporting.BLL.Computing;


namespace CastReporting.Reporting.Block.Table
{
    /// <summary>
    /// TechnicalSizingEvolution Class
    /// </summary>
    [Block("FUNCTIONAL_WEIGHT_EVOLUTION")]
    class FunctionalWeightEvolution : TableBlock
    {
         #region METHODS
        protected override TableDefinition Content(ReportData reportData, Dictionary<string, string> options)
        {
            TableDefinition resultTable = null;
            if (null != reportData &&
                null != reportData.CurrentSnapshot)
            {
                #region Declarations

                double? automatedFPoints = MeasureUtility.GetAutomatedIFPUGFunction(reportData.CurrentSnapshot);
                double? decisionPoints = MeasureUtility.GetDecisionPointsNumber(reportData.CurrentSnapshot);
                double? backFiredFPoints = MeasureUtility.GetBackfiredIFPUGFunction(reportData.CurrentSnapshot);

                double? automatedFPointsPrev = MeasureUtility.GetAutomatedIFPUGFunction(reportData.PreviousSnapshot);
                double? decisionPointsPrev = MeasureUtility.GetDecisionPointsNumber(reportData.PreviousSnapshot);
                double? backFiredFPointsPrev =MeasureUtility.GetBackfiredIFPUGFunction(reportData.PreviousSnapshot);

                double? automatedFPointsEvol = MathUtility.GetEvolution(automatedFPoints, automatedFPointsPrev);
                double? decisionPointsEvol = MathUtility.GetEvolution(decisionPoints, decisionPointsPrev);
                double? backFiredFPointsEvol = MathUtility.GetEvolution(backFiredFPoints, backFiredFPointsPrev);

                double? automatedFPointsPercent = MathUtility.GetPercent(automatedFPointsEvol, automatedFPointsPrev);
                double? decisionPointsPercent = MathUtility.GetPercent(decisionPointsEvol, decisionPointsPrev);
                double? backFiredFPointsPercent = MathUtility.GetPercent(backFiredFPointsEvol, backFiredFPointsPrev);

                bool hasPrevious = (reportData.PreviousSnapshot != null);

                #endregion

                const string noData = Constants.No_Data;
                const string metricFormat = "N0";

                var rowData = new List<string>() 
                    { Labels.Name, Labels.Current, Labels.Previous, Labels.Evolution, Labels.EvolutionPercent

                    , Labels.AutomatedFP
                    , automatedFPoints.HasValue? automatedFPoints.Value.ToString(metricFormat) : noData
                    , (hasPrevious && automatedFPointsPrev.HasValue)?  automatedFPointsPrev.Value.ToString(metricFormat) : noData
                    , (hasPrevious && automatedFPointsEvol.HasValue)? TableBlock.FormatEvolution((Int32)automatedFPointsEvol.Value) : noData
                    , (automatedFPointsPercent.HasValue)? TableBlock.FormatPercent(automatedFPointsPercent.Value): noData
                    , Labels.DecisionP
                    , decisionPoints.HasValue? decisionPoints.Value.ToString(metricFormat): noData
                    , (hasPrevious&& decisionPointsPrev.HasValue)? decisionPointsPrev.Value.ToString(metricFormat) : noData
                    , (hasPrevious && decisionPointsEvol.HasValue)? TableBlock.FormatEvolution((Int32)decisionPointsEvol.Value) : noData
                    , (decisionPointsPercent.HasValue)? TableBlock.FormatPercent(decisionPointsPercent.Value): noData
                    , Labels.BackfiredFP
                    , backFiredFPoints.HasValue? backFiredFPoints.Value.ToString(metricFormat) :noData
                    , (hasPrevious && backFiredFPointsPrev.HasValue)? backFiredFPointsPrev.Value.ToString(metricFormat) : noData
                    , (hasPrevious && backFiredFPointsEvol.HasValue)? TableBlock.FormatEvolution((Int32)backFiredFPointsEvol.Value) : noData
                    , (backFiredFPointsPercent.HasValue)? TableBlock.FormatPercent(backFiredFPointsPercent.Value): noData
                    };

                resultTable = new TableDefinition
                {
                    HasRowHeaders = false,
                    HasColumnHeaders = true,
                    NbRows = 6,
                    NbColumns = 5,
                    Data = rowData
                };
            }
            return resultTable;
        }
        #endregion METHODS
    }
}
