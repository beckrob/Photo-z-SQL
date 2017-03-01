
IF (OBJECT_ID('Util.ParseDoubleArray1D') IS NOT NULL)
BEGIN
    DROP FUNCTION [Util].[ParseDoubleArray1D]
END

GO


IF (OBJECT_ID('Util.ParseDoubleArray1DInvariant') IS NOT NULL)
BEGIN
    DROP FUNCTION [Util].[ParseDoubleArray1DInvariant]
END

GO


IF (OBJECT_ID('Util.ParseIntArray1D') IS NOT NULL)
BEGIN
    DROP FUNCTION [Util].[ParseIntArray1D]
END

GO


IF (OBJECT_ID('Util.ParseIntArray1DInvariant') IS NOT NULL)
BEGIN
    DROP FUNCTION [Util].[ParseIntArray1DInvariant]
END

GO


IF (OBJECT_ID('Util.GetTotalCLRMemoryUsage') IS NOT NULL)
BEGIN
    DROP FUNCTION [Util].[GetTotalCLRMemoryUsage]
END

GO


IF (OBJECT_ID('Config.SetupTemplateList_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupTemplateList_URL]
END

GO


IF (OBJECT_ID('Config.SetupTemplateList_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupTemplateList_ID]
END

GO


IF (OBJECT_ID('Config.SetupTemplateList_LuminositySpecified_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupTemplateList_LuminositySpecified_URL]
END

GO


IF (OBJECT_ID('Config.SetupTemplateList_LuminositySpecified_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupTemplateList_LuminositySpecified_ID]
END

GO


IF (OBJECT_ID('Config.SetupExtinctionLaw_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupExtinctionLaw_URL]
END

GO


IF (OBJECT_ID('Config.SetupExtinctionLaw_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupExtinctionLaw_ID]
END

GO


IF (OBJECT_ID('Config.RemoveExtinctionLaw') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[RemoveExtinctionLaw]
END

GO


IF (OBJECT_ID('Config.AddMissingValueSpecifier') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[AddMissingValueSpecifier]
END

GO


IF (OBJECT_ID('Config.ClearMissingValueSpecifiers') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[ClearMissingValueSpecifiers]
END

GO


IF (OBJECT_ID('Config.SetupFlatPrior') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupFlatPrior]
END

GO


IF (OBJECT_ID('Config.SetupAbsoluteMagnitudeLimitPrior_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupAbsoluteMagnitudeLimitPrior_URL]
END

GO


IF (OBJECT_ID('Config.SetupAbsoluteMagnitudeLimitPrior_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupAbsoluteMagnitudeLimitPrior_ID]
END

GO


IF (OBJECT_ID('Config.SetupBenitezHDFPrior_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupBenitezHDFPrior_URL]
END

GO


IF (OBJECT_ID('Config.SetupBenitezHDFPrior_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupBenitezHDFPrior_ID]
END

GO


IF (OBJECT_ID('Config.SetupTemplateTypePrior') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[SetupTemplateTypePrior]
END

GO


IF (OBJECT_ID('Config.RemoveInitialization') IS NOT NULL)
BEGIN
    DROP FUNCTION [Config].[RemoveInitialization]
END

GO


IF (OBJECT_ID('Compute.PhotoZBayesian_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Compute].[PhotoZBayesian_URL]
END

GO


IF (OBJECT_ID('Compute.PhotoZBayesian_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Compute].[PhotoZBayesian_ID]
END

GO


IF (OBJECT_ID('Compute.PhotoZMinChiSqr_URL') IS NOT NULL)
BEGIN
    DROP FUNCTION [Compute].[PhotoZMinChiSqr_URL]
END

GO


IF (OBJECT_ID('Compute.PhotoZMinChiSqr_ID') IS NOT NULL)
BEGIN
    DROP FUNCTION [Compute].[PhotoZMinChiSqr_ID]
END

GO


DROP ASSEMBLY [Jhu.PhotoZSQLDB]

GO


DROP ASSEMBLY [Jhu.PhotoZSQL]

GO


DROP ASSEMBLY [Jhu.PhotoZ]

GO


DROP ASSEMBLY [Jhu.SpecSvc.Util]

GO


DROP ASSEMBLY [MathNet.Numerics]

GO


DROP ASSEMBLY [System.Runtime.Serialization]

GO


DROP ASSEMBLY [SMDiagnostics]

GO


DROP ASSEMBLY [System.ServiceModel.Internals]

GO


DROP ASSEMBLY [TestMySpline]

GO


DROP ASSEMBLY [Jhu.SqlServer.Array]

GO


DROP ASSEMBLY [Jhu.SqlServer.Array.Parser]

GO


DROP SCHEMA [Util]

GO


DROP SCHEMA [Config]

GO


DROP SCHEMA [Compute]

GO

