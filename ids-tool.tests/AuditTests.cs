using CommandLine;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class AuditTests : BuildingSmartRepoFiles
{
    public AuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Theory]
    [MemberData(nameof(GetDevelopmentIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [Theory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.OmitContentAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [SkippableTheory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void AuditOfDocumentationPassFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        var knownFails = new[]
        {
            @"attribute\pass-an_optional_facet_always_passes_regardless_of_outcome_2_2.ids",
            @"attribute\pass-attributes_referencing_an_object_should_pass.ids",
            @"attribute\pass-attributes_with_a_boolean_false_should_pass.ids",
            @"attribute\pass-attributes_with_a_boolean_true_should_pass.ids",
            @"attribute\pass-attributes_with_a_zero_duration_should_pass.ids",
            @"attribute\pass-durations_are_treated_as_strings_1_2.ids",
            @"attribute\pass-integers_follow_the_same_rules_as_numbers.ids",
            @"attribute\pass-integers_follow_the_same_rules_as_numbers_2_2.ids",
            @"entity\pass-a_matching_predefined_type_should_pass.ids",
            @"entity\pass-a_predefined_type_may_specify_a_user_defined_element_type.ids",
            @"entity\pass-a_predefined_type_may_specify_a_user_defined_object_type.ids",
            @"entity\pass-a_predefined_type_may_specify_a_user_defined_process_type.ids",
            @"entity\pass-inherited_predefined_types_should_pass.ids",
            @"entity\pass-overridden_predefined_types_should_pass.ids",
            @"entity\pass-restrictions_an_be_specified_for_the_predefined_type_1_3.ids",
            @"entity\pass-restrictions_an_be_specified_for_the_predefined_type_2_3.ids",
            @"ids\pass-a_prohibited_specification_and_a_prohibited_facet_results_in_a_double_negative.ids",
            @"ids\pass-a_specification_passes_only_if_all_requirements_pass_2_2.ids",
            @"ids\pass-multiple_specifications_are_independent_of_one_another.ids",
            @"ids\pass-optional_specifications_may_still_pass_if_nothing_is_applicable.ids",
            @"ids\pass-prohibited_specifications_fail_if_at_least_one_entity_passes_all_requirements_1_3.ids",
            @"ids\pass-prohibited_specifications_fail_if_at_least_one_entity_passes_all_requirements_2_3.ids",
            @"ids\pass-required_specifications_need_at_least_one_applicable_entity_1_2.ids",
            @"ids\pass-specification_optionality_and_facet_optionality_can_be_combined.ids",
            @"partof\pass-a_group_predefined_type_must_match_exactly_2_2.ids",
            @"partof\pass-nesting_may_be_indirect.ids",
            @"partof\pass-the_container_predefined_type_must_match_exactly_2_2.ids",
            @"partof\pass-the_nest_entity_must_match_exactly_2_2.ids",
            @"partof\pass-the_nest_predefined_type_must_match_exactly_2_2.ids",
        };

        var testResult = knownFails.Any(x => f.FullName.EndsWith(x))
            ? Audit.Status.IdsContentError
            : Audit.Status.Ok;
        var expected = knownFails.Any(x => f.FullName.EndsWith(x))
            ? -1
            : 0;


        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAuditPattern = @"\\fail-",
            SchemaFiles = new[] { "/bsFiles/ids093.xsd" }
        };
        LoggerAndAuditHelpers.AuditWithOptions(c, XunitOutputHelper, testResult, expected);
#if DEBUG
        Skip.If(testResult != Audit.Status.Ok);
#endif 
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 6, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError)]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }

    [Theory]
    [InlineData("ValidFiles/nested_entity.ids")]
    public void FullAuditPass(string path)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }
}
