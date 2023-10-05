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
                        htm += '<td><a onclick=openFreelancerToFreelancerFeedbackPopup("' + subObj.FreelancerId + '",' + subObj.SolutionId + ',' + subObj.IndustryId +'); class="feedback-link">' + subObj.FreelancerName + '</a></td>';
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

function openFreelancerToFreelancerFeedbackPopup(freelancerId, solutionId,solutionId) {
    $("#FreelancerToFreelancerFeedbackModal").modal('show');
    $("#freelancer_activeId").val(freelancerId);
    $("#freelancer_solutionId").val(solutionId);
    $("#freelancer_industryId").val(solutionId);
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
            $('#preloader').hide();
            if (result.StatusCode != 200) {
                showToaster("error", "Error !", result.Message);
            } else {
                closeFreelancerToFreelacerFeedbackPopup();
                showToaster("success", "Success", result.Message);
            }
        },
        error: function (result) {
            $('#preloader').hide();
            //showToaster("success", "Success", "Review submitted successfully !");
            showToaster("error", "Error !", "Failed to submit !!");
        }
    });
    $('#preloader').hide();
}

function closeFreelancerToFreelacerFeedbackPopup() {
    $("#FreelancerToFreelancerFeedbackModal").modal('hide');
    $("#freelancer_activeId").val("");
    $("#freelancer_solutionId").val(0);
    $("#freelancer_industryId").val(0);
    $('input[name=freelancercollaborationteamwork-rating][value="0"]').prop('checked', true);
    $('input[name=freelancercommunication-rating][value="0"]').prop('checked', true);
    $('input[name=freelancerProfessionalism-rating][value="0"]').prop('checked', true);
    $('input[name=freelancertechnical-rating][value="0"]').prop('checked', true);
    $('input[name=freelancerprojectmanagement-rating][value="0"]').prop('checked', true);
    $('input[name=freelancerresponsiveness-rating][value="0"]').prop('checked', true);
    $('input[name=freelancerwelldefinedproject-rating][value="0"]').prop('checked', true);
    $("#freelancerfeedbackMessage").val("")
}