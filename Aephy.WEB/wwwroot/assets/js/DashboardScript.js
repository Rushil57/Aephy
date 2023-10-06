function OpenFreelancerSolutionTeamPopup(solutionFundId) {
    var data = {
        SolutionFundId: solutionFundId,
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetFreelancerListSolutionWise",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            $('#freelancerReviewListModal').modal('show');
            var data = result.Result
            if (data != "") {
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = data.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = data[index];

                        htm += '<tr>';
                        htm += '<td><a onclick=openFreelancerToFreelancerFeedbackPopup("' + subObj.FreelancerId + '",' + subObj.SolutionId + ',' + subObj.IndustryId + '); class="feedback-link">' + subObj.FreelancerName + '</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#solutionList tbody").append(htm);
                $(".cls-temp").remove();
            }
            else {
                $(".cls-temp").remove();
                $("<p class='cls-temp'>No Data Available</p>").insertAfter("#solutionList");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result);
        }
    });
}

function openFreelancerToFreelancerFeedbackPopup(freelancerId, solutionId, industryId) {
    //$("#FreelancerToFreelancerFeedbackModal").modal('show');
    $("#freelancer_activeId").val(freelancerId);
    $("#freelancer_solutionId").val(solutionId);
    $("#freelancer_industryId").val(industryId);
    var data = {
        SolutionId: solutionId,
        IndustryId: industryId,
        ToFreelancerId: freelancerId
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: "/Home/CheckFreelancerToFreelancerReviewExits",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Message == "Review Exists!") {
                BindFreelancerToFreelancerFeedbackModal(result.Result);
            }
            $('#FreelancerToFreelancerFeedbackModal').modal('show');
            $('#preloader').hide();
        },
        error: function (result) {
            $('#preloader').hide();
            //showToaster("success", "Success", "Review submitted successfully !");
            showToaster("error", "Error !", "Failed to submit !!");
        }
    });
}


function BindFreelancerToFreelancerFeedbackModal(freelancerReviewData) {
    $('#freelancerfeedbackMessage').val(freelancerReviewData.Feedback_Message).prop('disabled', 'disabled');
    $('input[name=freelancercollaborationteamwork-rating][value=' + freelancerReviewData.CollaborationAndTeamWork + ']').prop('checked', 'checked');
    $('input[name=freelancercommunication-rating][value=' + freelancerReviewData.Communication + ']').prop('checked', 'checked');
    $('input[name=freelancerProfessionalism-rating][value=' + freelancerReviewData.Professionalism + ']').prop('checked', 'checked');
    $('input[name=freelancertechnical-rating][value=' + freelancerReviewData.TechnicalSkills + ']').prop('checked', 'checked');
    $('input[name=freelancerprojectmanagement-rating][value=' + freelancerReviewData.ProjectManagement + ']').prop('checked', 'checked');
    $('input[name=freelancerresponsiveness-rating][value=' + freelancerReviewData.Responsiveness + ']').prop('checked', 'checked');
    $('input[name=freelancerwelldefinedproject-rating][value=' + freelancerReviewData.WellDefinedProjectScope + ']').prop('checked', 'checked');
    $('#freelancerReviewModalTitle').html('View Feedback');
    $('.freelancerTofreelancerFeedback_btn').prop('disabled', 'disabled');
    $('.rating__input').prop('disabled', 'disabled');
}


function SubmitFreelancerToFreelancerReview() {
    var collaborationTeamwork = $('input[name=freelancercollaborationteamwork-rating]:checked').val();
    var communicationskills = $('input[name=freelancercommunication-rating]:checked').val();
    var professionalismRating = $('input[name=freelancerProfessionalism-rating]:checked').val();
    var technicalRating = $('input[name=freelancertechnical-rating]:checked').val();
    var projectmanagementRating = $('input[name=freelancerprojectmanagement-rating]:checked').val();
    var responsivenessRating = $('input[name=freelancerresponsiveness-rating]:checked').val();
    var welldefinedProjectScopeRating = $('input[name=freelancerwelldefinedproject-rating]:checked').val();

    var SolutionTeamReviewData = {
        Id: $("#FreelancerToFreelnacerId").val(),
        ToFreelancerId: $("#freelancer_activeId").val(),
        SolutionId: $("#freelancer_solutionId").val(),
        IndustryId: $("#freelancer_industryId").val(),
        Feedback_Message: $("#freelancerfeedbackMessage").val(),
        CollaborationTeamWorkRating: collaborationTeamwork,
        CommunicationRating: communicationskills,
        ProfessionalismRating: professionalismRating,
        TechnicalRating: technicalRating,
        ProjectManagementRating: projectmanagementRating,
        ResponsivenessRating: responsivenessRating,
        WellDefinedProjectRating: welldefinedProjectScopeRating
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: '/Home/SaveFreelancerToFreelancerReview',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(SolutionTeamReviewData),
        success: function (result) {
            closeFreelancerToFreelacerFeedbackPopup();
            showToaster("success", "Success", result.Message);
            $('#preloader').hide();
        },
        error: function (result) {
            $('#preloader').hide();
            showToaster("error", "Error !", "Failed to submit !!");
        }
    });
}

function closeFreelancerToFreelacerFeedbackPopup() {
    $("#freelancer_activeId").val("");
    $("#freelancer_solutionId").val(0);
    $("#freelancer_industryId").val(0);
    $("#freelancerfeedbackMessage").val("")
    ResetFreelancerReview();
    $("#FreelancerToFreelancerFeedbackModal").modal('hide');
}

function ResetFreelancerReview() {
    $('input[name=freelancercollaborationteamwork-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancercommunication-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancerProfessionalism-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancertechnical-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancerprojectmanagement-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancerresponsiveness-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=freelancerwelldefinedproject-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $("#freelancerfeedbackMessage").val("");
    $('#freelancerfeedbackMessage').prop('disabled', false);
    $('.freelancerTofreelancerFeedback_btn').prop('disabled', false);
    $('.rating__input').prop('disabled', false);
}