using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using System;
using System.Data;

namespace boilersGraphics.Dao
{
    class StatisticsDao : Dao<Statistics>
    {
        public StatisticsDao()
            : base()
        { }

        public StatisticsDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Statistics ToEntity(IDataRecord reader)
        {
            return new Statistics()
            {
                ID = reader.SafeGetGuid("ID", Table),
                NumberOfBoots = reader.SafeGetInt("NumberOfBoots", Table),
                UptimeTicks = reader.SafeGetLong("UptimeTicks", Table),
                NumberOfTimesTheFileWasOpenedBySpecifyingIt = reader.SafeGetInt("NumberOfTimesTheFileWasOpenedBySpecifyingIt", Table),
                NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened = reader.SafeGetInt("NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened", Table),
                NumberOfClicksWithThePointerTool = reader.SafeGetInt("NumberOfClicksWithThePointerTool", Table),
                CumulativeTotalOfItemsSelectedWithTheLassoTool = reader.SafeGetInt("CumulativeTotalOfItemsSelectedWithTheLassoTool", Table),
                NumberOfDrawsOfTheStraightLineTool = reader.SafeGetInt("NumberOfDrawsOfTheStraightLineTool", Table),
                NumberOfDrawsOfTheRectangleTool = reader.SafeGetInt("NumberOfDrawsOfTheRectangleTool", Table),
                NumberOfDrawsOfTheEllipseTool = reader.SafeGetInt("NumberOfDrawsOfTheEllipseTool", Table),
                NumberOfDrawsOfTheImageFileTool = reader.SafeGetInt("NumberOfDrawsOfTheImageFileTool", Table),
                NumberOfDrawsOfTheLetterTool = reader.SafeGetInt("NumberOfDrawsOfTheLetterTool", Table),
                NumberOfDrawsOfTheVerticalLetterTool = reader.SafeGetInt("NumberOfDrawsOfTheVerticalLetterTool", Table),
                NumberOfDrawsOfPolygonTool = reader.SafeGetInt("NumberOfDrawsOfPolygonTool", Table),
                NumberOfDrawsOfBezierCurveTool = reader.SafeGetInt("NumberOfDrawsOfBezierCurveTool", Table),
                NumberOfSnapPointToolInstallations = reader.SafeGetInt("NumberOfSnapPointToolInstallations", Table),
                BrushToolDrawCount = reader.SafeGetInt("BrushToolDrawCount", Table),
                NumberOfTimesTheEraserToolHasBeenUsed = reader.SafeGetInt("NumberOfTimesTheEraserToolHasBeenUsed", Table),
                NumberOfTimesSaved = reader.SafeGetInt("NumberOfTimesSaved", Table),
                NumberOfTimesYouHaveNamedAndSaved = reader.SafeGetInt("NumberOfTimesYouHaveNamedAndSaved", Table),
                NumberOfExports = reader.SafeGetInt("NumberOfExports", Table),
                NumberOfJpegExports = reader.SafeGetInt("NumberOfJpegExports", Table),
                NumberOfPngExports = reader.SafeGetInt("NumberOfPngExports", Table),
                NumberOfGifExports = reader.SafeGetInt("NumberOfGifExports", Table),
                NumberOfBmpExports = reader.SafeGetInt("NumberOfBmpExports", Table),
                NumberOfTiffExports = reader.SafeGetInt("NumberOfTiffExports", Table),
                NumberOfWmpExports = reader.SafeGetInt("NumberOfWmpExports", Table),
                NumberOfTimesGrouped = reader.SafeGetInt("NumberOfTimesGrouped", Table),
                NumberOfUngrouped = reader.SafeGetInt("NumberOfUngrouped", Table),
                NumberOfMovesToTheFrontend = reader.SafeGetInt("NumberOfMovesToTheFrontend", Table),
                NumberOfMovesToTheFront = reader.SafeGetInt("NumberOfMovesToTheFront", Table),
                NumberOfMovesToTheBack = reader.SafeGetInt("NumberOfMovesToTheBack", Table),
                NumberOfMovesToTheBackend = reader.SafeGetInt("NumberOfMovesToTheBackend", Table),
                NumberOfTopAlignment = reader.SafeGetInt("NumberOfTopAlignment", Table),
                NumberOfTimesTheTopAndBottomAreCentered = reader.SafeGetInt("NumberOfTimesTheTopAndBottomAreCentered", Table),
                NumberOfBottomAlignment = reader.SafeGetInt("NumberOfBottomAlignment", Table),
                NumberOfLeftAlignment = reader.SafeGetInt("NumberOfLeftAlignment", Table),
                NumberOfTimesLeftAndRightCentered = reader.SafeGetInt("NumberOfTimesLeftAndRightCentered", Table),
                NumberOfRightAlignment = reader.SafeGetInt("NumberOfRightAlignment", Table),
                NumberOfTimesAlignedLeftAndRight = reader.SafeGetInt("NumberOfTimesAlignedLeftAndRight", Table),
                NumberOfTimesAlignedUpAndDown = reader.SafeGetInt("NumberOfTimesAlignedUpAndDown", Table),
                NumberOfTimesToMatchTheWidth = reader.SafeGetInt("NumberOfTimesToMatchTheWidth", Table),
                NumberOfTimesToMatchTheHeight = reader.SafeGetInt("NumberOfTimesToMatchTheHeight", Table),
                NumberOfUnions = reader.SafeGetInt("NumberOfUnions", Table),
                NumberOfIntersects = reader.SafeGetInt("NumberOfIntersects", Table),
                NumberOfXors = reader.SafeGetInt("NumberOfXors", Table),
                NumberOfExcludes = reader.SafeGetInt("NumberOfExcludes", Table),
                NumberOfCuts = reader.SafeGetInt("NumberOfCuts", Table),
                NumberOfCopies = reader.SafeGetInt("NumberOfCopies", Table),
                NumberOfPasted = reader.SafeGetInt("NumberOfPasted", Table),
                NumberOfUndos = reader.SafeGetInt("NumberOfUndos", Table),
                NumberOfRedoes = reader.SafeGetInt("NumberOfRedoes", Table),
                NumberOfTimesAutomaticallySaved = reader.SafeGetInt("NumberOfTimesAutomaticallySaved", Table),
                NumberOfNewlyCreatedLayers = reader.SafeGetInt("NumberOfNewlyCreatedLayers", Table),
                NumberOfDeletedLayers = reader.SafeGetInt("NumberOfDeletedLayers", Table),
                NumberOfTimesTheItemWasDrawn = reader.SafeGetInt("NumberOfTimesTheItemWasDrawn", Table),
                NumberOfTimesTheItemWasDeleted = reader.SafeGetInt("NumberOfTimesTheItemWasDeleted", Table),
                NumberOfLogLevelChanges = reader.SafeGetInt("NumberOfLogLevelChanges", Table),
                NumberOfTimesTheVersionInformationDialogWasDisplayed = reader.SafeGetInt("NumberOfTimesTheVersionInformationDialogWasDisplayed", Table),
                NumberOfTimesTheApplicationLogWasDisplayed = reader.SafeGetInt("NumberOfTimesTheApplicationLogWasDisplayed", Table),
                NumberOfTimesSliceToolHasBeenUsed = reader.SafeGetInt("NumberOfTimesSliceToolHasBeenUsed", Table),
                NumberOfDrawsOfFreeHandTool = reader.SafeGetInt("NumberOfDrawsOfFreeHandTool", Table),
                NumberOfDrawsOfThePieTool = reader.SafeGetInt("NumberOfDrawsOfThePieTool", Table),
            };
        }
    }
}
