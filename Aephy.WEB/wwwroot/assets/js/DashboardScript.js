﻿function OpenFreelancerSolutionTeamPopup(solutionFundId) {
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
            showToaster("error", "Error !", result.responseText);
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
            showToaster("error", "Error !", result.responseText);
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
            showToaster("error", "Error !", result.responseText);
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

function makeTableScroll() {
    var maxRows = 5;

    var table = document.getElementById('table-OpenRoles');
    var wrapper = table.parentNode;
    var rowsInTable = table.rows.length;
    var height = 0;
    if (rowsInTable > maxRows) {
        for (var i = 0; i < maxRows + 1; i++) {
            height += table.rows[i].clientHeight;
        }
        wrapper.style.height = height + "px";
    }

    height = 0;
    var table_Approve = document.getElementById('ApprovedList');
    var wrapper_Approve = table_Approve.parentNode;
    var rowsInTable_Approve = table_Approve.rows.length;
    var height_Approve = 0;
    if (rowsInTable_Approve > maxRows) {
        for (var i = 0; i < maxRows + 1; i++) {
            height += table_Approve.rows[i].clientHeight;
        }
        wrapper_Approve.style.height = height + "px";
    }
}

function SubmitFreelancerFeedback() {

    var freelancerId = $('#FreelancerID').val();
    var Industry = $('#Industry_Id').val();
    var Solution = $('#Solution_Id').val();
    var message = $('#feedbackMessage').val();
    var communicationRating = $('input[name=communication-rating]:checked').val();
    var collaborationRating = $('input[name=collaboration-rating]:checked').val();
    var professionalismRating = $('input[name=Professionalism-rating]:checked').val();
    var technicalRating = $('input[name=technical-rating]:checked').val();
    var satisfactionRating = $('input[name=satisfaction-rating]:checked').val();
    var responsivenessRating = $('input[name=responsiveness-rating]:checked').val();
    var likeToWorkRating = $('input[name=likeToWork-rating]:checked').val();

    var SolutionTeamReviewData = {
        FreelancerId: freelancerId,
        SolutionId: Solution,
        IndustryId: Industry,
        Feedback_Message: message,
        CommunicationRating: communicationRating,
        CollaborationRating: collaborationRating,
        ProfessionalismRating: professionalismRating,
        TechnicalRating: technicalRating,
        SatisfactionRating: satisfactionRating,
        ResponsivenessRating: responsivenessRating,
        LikeToWorkRating: likeToWorkRating
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: '/Home/SaveFreelancerReviewData',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(SolutionTeamReviewData),
        success: function (result) {
            $('#preloader').hide();
            if (result.StatusCode != 200) {
                showToaster("error", "Error !", result.Message);
            } else {
                closeFreelancerFeedbackPopup();
                showToaster("success", "Success", result.Message);
            }
        },
        error: function (result) {
            $('#preloader').hide();
            showToaster("error", "Error !", result.responseText);
        }
    });
    $('#preloader').hide();

    //alert('data:\n'+ message +'\n'+ communicationRating +'\n'+collaborationRating +'\n'+ ProfessionalismRating +'\n'+ technicalRating+'\n'+ satisfactionRating +'\n'+ responsivenessRating +'\n'+ likeToWorkRating+'\n'+ wellDefinedRating +'\n'+ adherenceRating +'\n'+ deliverablesQualityRating+'\n'+ meetingTimelinessRating +'\n'+ clientsatisfactionRating +'\n'+ adherenceToBudgetRating+'\n'+ likeToRecommendRating)
    //alert('data: \n'+ message +'\n'+ rating)
}

function SubmitSolutionFeedback() {
    var solutionId = $('#SolutionID').val();
    var industryId = $('#IndustryID').val();
    var message = $('#Message').val();
    var wellDefinedRating = $('input[name=wellDefined-rating]:checked').val();
    var adherenceRating = $('input[name=adherence-rating]:checked').val();
    var deliverablesQualityRating = $('input[name=deliverablesQuality-rating]:checked').val();
    var meetingTimelinessRating = $('input[name=MeetingTimeliness-rating]:checked').val();
    var clientsatisfactionRating = $('input[name=clientsatisfaction-rating]:checked').val();
    var adherenceToBudgetRating = $('input[name=adherenceToBudget-rating]:checked').val();
    var likeToRecommendRating = $('input[name=likeToRecommend-rating]:checked').val();

    var SolutionReviewData = {
        SolutionId: solutionId,
        IndustryId: industryId,
        Feedback_Message: message,
        WellDefinedProjectScope: wellDefinedRating,
        AdherenceToProjectScope: adherenceRating,
        DeliverablesQuality: deliverablesQualityRating,
        MeetingTimeliness: meetingTimelinessRating,
        Clientsatisfaction: clientsatisfactionRating,
        AdherenceToBudget: adherenceToBudgetRating,
        LikeToRecommend: likeToRecommendRating
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: '/Home/SaveSolutionReviewData',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(SolutionReviewData),
        success: function (result) {
            $('#preloader').hide();
            if (result.StatusCode != 200) {
                showToaster("error", "Error !", result.Message);
            } else {
                CloseSolutionPopup();
                showToaster("success", "Success", result.Message);
            }
        },
        error: function (result) {
            $('#preloader').hide();
            showToaster("error", "Error !", result.responseText);
        }
    });
    $('#preloader').hide();

    //alert('data:\n'+ message +'\n'+ communicationRating +'\n'+collaborationRating +'\n'+ ProfessionalismRating +'\n'+ technicalRating+'\n'+ satisfactionRating +'\n'+ responsivenessRating +'\n'+ likeToWorkRating+'\n'+ wellDefinedRating +'\n'+ adherenceRating +'\n'+ deliverablesQualityRating+'\n'+ meetingTimelinessRating +'\n'+ clientsatisfactionRating +'\n'+ adherenceToBudgetRating+'\n'+ likeToRecommendRating)
    //alert('data: \n'+ message +'\n'+ rating)
}

function GigOpenRolesApply() {

    var filename = $("#CVPath").val();
    var validFileExtentions = /(\.pdf|\.docx)$/i;
    if (validFileExtentions.exec(filename)) {
        $('#preloader').show();
        var fileUpload = $("#CVPath").get(0);
        var files = fileUpload.files;

        var GigOpenRolesData = {
            FreelancerID: 0,
            GigOpenRoleId: $("#gigRole_id").val(),
            IsApproved: false,
            Description: $("#Description").val(),
            AlreadyExistCv: $("#ChangeCvUpload").val()
        }

        var formData = new FormData();
        for (var i = 0; i < files.length; i++) {
            // fileData.append(files[i].name, files[i]);
            formData.append("httpPostedFileBase", files[i]);
        }
        formData.append("GigOpenRolesData", JSON.stringify(GigOpenRolesData));

        $.ajax({
            type: "POST",
            url: '/Home/ApplyForOpenGigRoles',
            contentType: false,
            processData: false,
            data: formData,
            success: function (result) {
                clearApplyForm();
                $('#preloader').hide();
                //alert(result)
                showToaster("success", "Success", result);
                CloseGigOpeningApplyModal();
            },
            error: function (result) {
                clearApplyForm();
                $('#preloader').hide();
                showToaster("error", "Error !", result.responseText);

            }
        });
    } else {
        //alert('You can upload only PDF or Document file..')
        showToaster("error", "Error !", "You can upload only PDF or Document file..");
        clearApplyForm();
    }
}

// code to read selected table row cell data (values).
function GigApply(Id) {
    OpenGigOpeningApplyModal();
    clearApplyForm();
    $("#gigRole_id").val(Id);
    $.ajax({
        type: "post",
        url: '/LandingPage/GetRolesDataById',
        dataType: "json",
        data: { ID: Id },
        success: function (result) {
            var data = result.Result;
            $("#RoleDescription").val(data.OpenRoles.Description);
            if (data.FreelancerDetail.CVUrlWithSas != null) {
                $("#dwld-resume").show()
                $("#user-CvPath").val(data.FreelancerDetail.CVPath)
                addNameToFile(data.FreelancerDetail.CVPath.split("/")[4])
            }
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function addNameToFile(url) {
    getImgURL(url, (imgBlob) => {
        let file = new File([imgBlob], url, { type: "pdf/application", lastModified: new Date().getTime() }, 'utf-8');
        let container = new DataTransfer();
        container.items.add(file);
        document.querySelector('#CVPath').files = container.files;
    })
}

function getImgURL(url, callback) {
    var xhr = new XMLHttpRequest();
    xhr.onload = function () {
        callback(xhr.response);
    };
    xhr.open('GET', url);
    xhr.responseType = 'blob';
    xhr.send();
}

function OpenGigOpeningApplyModal() {
    $("#GigOpeningApplyModal").modal('show')
}

function GetApprovedList() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetApprovedList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            var index = 0;
            var subObj = '';
            var htm = '';
            var resultLength = result.Result.length;
            if (resultLength > 0) {
                for (index = 0; index < resultLength; index++) {
                    subObj = result.Result[index];

                    htm += '<tr>';
                    htm += '<td class="cls-RolesId" id="hidden_item">' + subObj.ID + '</td>';
                    htm += '<td>' + subObj.ServiceName + '</td>';
                    htm += '<td>' + subObj.SolutionName + '</td>';
                    htm += '<td id="hidden_item">' + subObj.Title + '</td>';
                    htm += '<td class="cls-level">' + subObj.Level + '</td>';
                    htm += '<td>' + subObj.IndustryName + '</td>';
                    htm += '<td><a class="btn btn-danger pt-0 pb-1" style="color: white;" onclick=DeleteFreelancerAppliedSolution(this,' + subObj.SolutionId + ',' + subObj.IndustryId + ')>Delete</a></td>';
                    //if (subObj.IsDefine) {
                    //    htm += '<td><a class="btn btn-success applyFor-btn pt-0 pb-1" style="color: white;" onclick="OpenDetailsPopModal(' + subObj.ID + ',' + subObj.SolutionId + ',' + subObj.IndustryId + ')">Define</a></td>';
                    //}
                    htm += '</tr>';
                }
            } else {
                htm = '<tr><td colspan="7"><center>No Data Available</center></td><tr>';
            }
            $("#ApprovedList").find("tr:gt(0)").remove();
            $("#ApprovedList tbody").append(htm);
            $("#preloader").hide();
        },
        error: function (result) {
            var htm = '<tr><td colspan="7"><center>No Data Available</center></td><tr>';
            $("#ApprovedList").find("tr:gt(0)").remove();
            $("#ApprovedList tbody").append(htm);
            showToaster("error", "Error !", result.responseText);
            $("#preloader").hide();
        }
    });
}

function GetFreelancerRequestList() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetRequestedList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            var index = 0;
            var subObj = '';
            var htm = '';
            var resultLength = result.Result.length;
            if (resultLength > 0) {
                for (index = 0; index < resultLength; index++) {
                    subObj = result.Result[index];

                    htm += '<tr>';
                    htm += '<td class="cls-RolesId" id="hidden_item">' + subObj.ID + '</td>';
                    htm += '<td>' + subObj.SolutionName + '</td>';
                    htm += '<td>' + subObj.IndustriesName + '</td>';
                    if (subObj.ApproveStatus == 0) {
                        htm += '<td><a class="btn btn-success pt-0 pb-1" style="color: white;" onclick=RequestApproveReject(' + subObj.DetailId + ',1)>Approve</a> &nbsp;' +
                            '<a class="btn btn-danger pt-0 pb-1" style="color: white;" onclick=RequestApproveReject(' + subObj.DetailId + ',2)>Reject</a></td>';
                    }
                    else {
                        if (subObj.ApproveStatus == 1) {
                            htm += '<td><span class="badge bg-success">Accept</span></td>';
                        }
                        else if (subObj.ApproveStatus == 2) {
                            htm += '<td><span class="badge bg-danger">Rejected</span></td>';
                        }
                        else {
                            htm += '<td></td>';
                        }
                    }
                    htm += '</tr>';
                }
            } else {
                htm = '<tr><td colspan="7"><center>No Data Available</center></td><tr>';
            }
            $("#RequestList").find("tr:gt(0)").remove();
            $("#RequestList tbody").append(htm);
            $("#preloader").hide();
        },
        error: function (result) {
            var htm = '<tr><td colspan="7"><center>No Data Available</center></td><tr>';
            $("#ApprovedList").find("tr:gt(0)").remove();
            $("#ApprovedList tbody").append(htm);
            showToaster("error", "Error !", result.responseText);
            $("#preloader").hide();
        }
    });
}

function DeleteFreelancerAppliedSolution(data, SolutionId, IndustryId) {
    var Freelancerlevel = $(data).closest('tr').find('.cls-level').text();
    Swal.fire({
        title: 'Are you sure you want to delete solution?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then(function (t) {
        if (!t.isConfirmed) return;

        var data = {
            IndustryId: IndustryId,
            SolutionId: SolutionId,
            FreelancerLevel: Freelancerlevel
        };
        $("#preloader").show();
        $.ajax({
            type: "POST",
            url: "/Home/DeleteFreelancerSolution",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data),
            success: function (result) {
                showToaster("success", "Success", result.Message);
                GetApprovedList()
                $("#preloader").hide();
            },
            error: function (result) {
                $("#preloader").hide();
                showToaster("error", "Error !", result.responseText);
            }
        });
    });
}

function RequestApproveReject(id, status) {

    Swal.fire({
        title: 'Are you sure ?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, do it!'
    }).then(function (t) {
        if (!t.isConfirmed) return;
        var data = {
            Id: id,
            RequestStatus: status,
        };

        $("#preloader").show();
        $.ajax({
            type: "POST",
            url: "/Home/FreelancerRequest",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data),
            success: function (result) {
                if (result.Message == "Succesfully") {
                    showToaster("success", "Success", "Action taken successfully.");
                    GetFreelancerRequestList();
                    $("#preloader").hide();
                }
                else {
                    showToaster("error", "Error", result.Message);
                    GetFreelancerRequestList();
                    $("#preloader").hide();
                }
            },
            error: function (result) {
                $("#preloader").hide();
                showToaster("error", "Error !", result);
            }
        });

    });
}

function DownloadCv() {
    var Cvname = $("#user-CvPath").val()
    if (Cvname != "") {
        window.location = "/Home/DownloadApplicantCV?Cvname=" + Cvname;
    }
}

function GotoProjectPage(serviceId, solutionId, industryId) {
    window.location.href = '/LandingPage/Project?Service=' + serviceId + '&Solution=' + solutionId + '&Industry=' + industryId;
}

function GetFreelancerArchivesProject() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetFreelancerArchivesProject",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var archivesData = result.Result
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = archivesData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = archivesData[index];
                        if (subObj.MileStoneTitle == null) {
                            subObj.MileStoneTitle = ""
                        }

                        htm += '<tr>';
                        htm += '<td>' + subObj.Title + '</td>';
                        htm += '<td>' + subObj.Industries + '</td>';
                        htm += '<td>' + subObj.MileStoneTitle + '</td>';
                        htm += '<td><a class="feedback-link cls-freelancerField" onclick="OpenFreelancerSolutionTeamPopup(' + subObj.Id + ');">Feedback</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#ArchieveTable").find("tr:gt(0)").remove();
                $("#ArchieveTable tbody").append(htm);
                $(".cls-archive-temp").hide();
            }
            else {
                $(".cls-archive-temp").remove();
                $("<p class='cls-archive-temp'>No Data Available</p>").insertAfter("#ArchieveTable");
            }
            $("#preloader").hide();

            $("#preloader").hide();

        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function GetClientArchivesProject() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetArchivesProject",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var archivesData = result.Result
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = archivesData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = archivesData[index];
                        if (subObj.MileStoneTitle == null) {
                            subObj.MileStoneTitle = ""
                        }

                        htm += '<tr>';
                        htm += '<td>' + subObj.Title + '</td>';
                        htm += '<td>' + subObj.Industries + '</td>';
                        htm += '<td>' + subObj.MileStoneTitle + '</td>';
                        htm += '<td><a class="feedback-link cls-clientField" onclick="OpenSolutionTeamPopup(' + subObj.Id + ',' + subObj.ServiceId + ',' + subObj.SolutionId + ',' + subObj.IndustryId + ');">Freelancers Feedback</a></td>';
                        htm += '<td><a class="feedback-link cls-clientField" onclick="OpenSolutionPopup(' + subObj.Id + ',' + subObj.ServiceId + ',' + subObj.SolutionId + ',' + subObj.IndustryId + ');">Solution Feedback</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#ArchieveTable tbody").append(htm);
                $(".cls-archive-temp").hide();
            }
            else {
                $(".cls-archive-temp").remove();
                $("<p class='cls-archive-temp'>No Data Available</p>").insertAfter("#ArchieveTable");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

//getExpense
function getClientProjectExpense() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetClientProjectExpense",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var Data = result.Result

                $("#ContractUser-Value").html(Data.Projects);
                if (Data.Expense == 0) {
                    $('#expense_amount').html(0);
                } else {
                    $('#expense_amount').html(Data.CurrentCurrency + formatNumber(Data.Expense.toFixed()));
                }
                
                $('#Project_Duartion').html(Data.ProjectDuartion + " Days");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function getFreelancerProjectExpense() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/getFreelancerProjectExpense",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var Data = result.Result

                $("#ContractUser-Value").html(Data.FreelancerActiveProject);
                if (Data.RevenueAmount == 0) {
                    $('#revenue_amount').html(0);
                } else {
                    $('#revenue_amount').html(Data.FreelancerCurrency + formatNumber(Data.RevenueAmount));
                }
                
                $('#Project_Duartion').html(Data.ProjectDuration + " Days");
            } else {
                $("#ContractUser-Value").html(0);
                $('#revenue_amount').html(0);
                $('#Project_Duartion').html("0 Days");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function formatNumber(n) {
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",")
}

function GetActiveProjectInvoices() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetActiveProjectInvoices",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var invoiceData = result.Result
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = invoiceData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = invoiceData[index];
                        if (subObj.MileStoneTitle == null) {
                            subObj.MileStoneTitle = ""
                        }

                        htm += '<tr>';
                        htm += '<td>' + subObj.Title + '</td>';
                        htm += '<td>' + subObj.Industries + '</td>';
                        htm += '<td>' + subObj.MileStoneTitle + '</td>';
                        htm += '<td><a class="text-primary" onclick=ViewInvoiceGridPopUp(' + subObj.ContractId + ')>View Invoice</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#InvoiceTable").find("tr:gt(0)").remove();
                $("#InvoiceTable tbody").append(htm);
                $(".cls-invoice-temp").hide();
            }
            else {
                $(".cls-invoice-temp").remove();
                $("<p class='cls-invoice-temp'>No Data Available</p>").insertAfter("#InvoiceTable");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function GetFreelancerInvoices() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetFreelancerInvoices",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result.length != 0) {
                var invoiceData = result.Result
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = invoiceData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = invoiceData[index];
                        if (subObj.MileStoneTitle == null) {
                            subObj.MileStoneTitle = ""
                        }

                        htm += '<tr>';
                        htm += '<td>' + subObj.Title + '</td>';
                        htm += '<td>' + subObj.Industries + '</td>';
                        htm += '<td>' + subObj.MileStoneTitle + '</td>';
                        htm += '<td><a class="text-primary" onclick=ViewFreelancerInvoiceGridPopUp(' + subObj.ContractId + ')>View Invoice</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#FreelancerInvoiceTable").find("tr:gt(0)").remove();
                $("#FreelancerInvoiceTable tbody").append(htm);
                $(".cls-Freelancerinvoice-temp").hide();
            }
            else {
                $(".cls-Freelancerinvoice-temp").remove();
                $("<p class='cls-Freelancerinvoice-temp'>No Data Available</p>").insertAfter("#FreelanerInvoiceTable");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function DownloadSolutionInvoice() {
    $("#preloader").show();
    var contractId = $("#InvoiceId").val();
    window.location = "/Home/DownloadInvoice?InvoiceId=" + contractId;

    // $("#preloader").hide();
    setTimeout(function () {
        $("#preloader").hide();
    }, 10000);
}

function OpenInvoiceModalPopUp(invoiceId) {
    var data = {
        InvoiceId: invoiceId,
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetClientInvoiceDetails",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Result != null) {
                $("#InvoiceModal").modal('show')
                var data = result.Result;

                var totalamt = parseFloat(data.TotalAmount).toFixed(2);
                var splitamt = totalamt.toString().split(".");
                splitamt[0] = splitamt[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                var finalamt = splitamt.join(".");

                $("#ContractCreatedDate").html("Date " + moment(data.InvoiceDate).format('DD MMMM YYYY'));
                $("#ContractDueDate").html("Due Date " + moment(data.InvoiceDate).format('DD MMMM YYYY'));
                $("#ContractClientName").html(data.ClientFullName);
                $("#ProjectTotalAmount").html("Total Amount " + data.PreferredCurrency + finalamt);
                $("#ProjectTotalDueAmount").html("Due Amount " + data.PreferredCurrency + finalamt);
                $("#ProjectTotalDueAmount").css("font-weight", "bold");
                $("#ContractDueDate").css("font-weight", "bold");
                if (data.ClientAddress == null) {
                    data.ClientAddress = "";
                }
                $("#ClientAddress").html(data.ClientAddress)

                if (data.ClientCompanyName == null) {
                    data.ClientCompanyName = "";
                }
                $("#ClientCompanyName").html("Company Name : " + data.ClientCompanyName)

                if (data.InvoicelistDetails.length != 0) {
                    var index = 0;
                    var subObj = '';
                    var htm = '';
                    var resultLength = data.InvoicelistDetails.length;
                    if (resultLength > 0) {
                        for (index = 0; index < resultLength; index++) {
                            subObj = data.InvoicelistDetails[index];
                            if (!subObj.Amount.indexOf("(") == 0) {
                                if (subObj.Amount == "") {
                                    subObj.Amount = 0.0;
                                } else {
                                    var totalinvoiceamt = parseFloat(subObj.Amount).toFixed(2);
                                    var splitinvoiceamt = totalinvoiceamt.toString().split(".");
                                    splitinvoiceamt[0] = splitinvoiceamt[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                                    subObj.Amount = splitinvoiceamt.join(".");
                                }
                            }
                            htm += '<tr>';
                            htm += '<td>' + subObj.Description + '</td>';
                            htm += '<td>' + subObj.Amount + '</td>';
                            htm += '</tr>';
                        }
                    }
                    $("#InvoiceListDetails").find("tr:gt(0)").remove();
                    $("#InvoiceListDetails tbody").append(htm);
                }


                if (data.TransactionType == "Payment Receipt - Amount Due") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#ContractDueDate").css("display", "none");
                    $("#ProjectTotalDueAmount").css("display", "none");
                    $("#invoice-mainTitle").html("RECEIPT");
                    $(".cls-ModelInvoiceMainTitle").html("RECEIPT");
                    $("#billTo-tile").html("Received From");
                    $("#InvoiceNumber").html("Receipt # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice1 - portal to client") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#invoice-mainTitle").html("Invoice");
                    $(".cls-ModelInvoiceMainTitle").html("Invoice");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice3 - Total platform fees") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#invoice-mainTitle").html("PROFORMA INVOICE");
                    $(".cls-ModelInvoiceMainTitle").html("PROFORMA INVOICE");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "CREDIT MEMO") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#invoice-mainTitle").html("CREDIT MEMO");
                    $(".cls-ModelInvoiceMainTitle").html("CREDIT MEMO");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "none");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice Freelancer") {

                    if (data.FreelancerAddress == null) {
                        data.FreelancerAddress = "";
                    }

                    $("#invoiceCeated-title").css("display", "block");
                    $("#invoiceCeated-title").css("margin-left", "4%");
                    $("#invoiceCeated-title").css("margin-top", "4%");

                    $("#from-Title").html(data.FreelancerFullName);
                    $("#invoice-mainTitle").html("INVOICE");
                    $(".cls-ModelInvoiceMainTitle").html("INVOICE");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);
                    $("#from-name").html("Name :" + data.FreelancerFullName);
                    $("#from-address").html("Address : " + data.FreelancerAddress);
                    $("#from-country").css("display", "none");

                    if (data.FreelancerCountry == "GR") {
                        if (data.FreelancerTaxType != "" && data.FreelancerTaxType != null && data.FreelancerTaxType != "null") {
                            $("#from-vatId").css("display", "block");
                            $("#from-vatId").html(data.FreelancerTaxType + "ID : " + data.FreelancerTaxId);
                        }
                    }
                    else {
                        $("#from-vatId").css("display", "none");
                    }
                }
                if (data.TransactionType == "Invoice Commision") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-Title").html("Ephylink");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                }

                if (data.ClientCountry == "GR") {
                    $("#TaxDetails").css("display", "block");
                    if (data.TaxType != "" && data.TaxType != null && data.TaxType != "null") {
                        $("#TaxDetails").html(data.TaxType + " ID : " + data.TaxId)
                    }
                } else {
                    $("#TaxDetails").css("display", "none");
                }
            }

            $("#from-Title").css("font-size", "19px");
            $("#from-Title").css("font-weight", "bold");
            $("#ContractClientName").css("font-size", "19px");
            $("#ContractClientName").css("font-weight", "bold");

            $("#InvoiceId").val(invoiceId);
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result);
        }
    });
}

function OpenFreelancerInvoiceModalPopUp(invoiceId) {
    var data = {
        InvoiceId: invoiceId,
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetFreelancerInvoiceDetails",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Result != null) {
                $("#InvoiceModal").modal('show')
                var data = result.Result;

                var totalamt = parseFloat(data.TotalAmount).toFixed(2);
                var splitamt = totalamt.toString().split(".");
                splitamt[0] = splitamt[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                var finalamt = splitamt.join(".");

                $("#ContractCreatedDate").html("Date " + moment(data.InvoiceDate).format('DD MMMM YYYY'));
                $("#ContractDueDate").html("Due Date " + moment(data.InvoiceDate).format('DD MMMM YYYY'));
                $("#ContractClientName").html(data.ClientFullName);
                $("#ProjectTotalAmount").html("Total Amount " + data.PreferredCurrency + finalamt);
                $("#ProjectTotalDueAmount").html("Due Amount " + data.PreferredCurrency + finalamt);
                $("#ProjectTotalDueAmount").css("font-weight", "bold");
                $("#ContractDueDate").css("font-weight", "bold");
                if (data.ClientAddress == null) {
                    data.ClientAddress = "";
                }
                $("#ClientAddress").html("Address : " + data.ClientAddress)
                if (data.InvoicelistDetails.length != 0) {
                    var index = 0;
                    var subObj = '';
                    var htm = '';
                    var resultLength = data.InvoicelistDetails.length;
                    if (resultLength > 0) {
                        for (index = 0; index < resultLength; index++) {
                            subObj = data.InvoicelistDetails[index];
                            if (!subObj.Amount.indexOf("(") == 0) {
                                if (subObj.Amount == "") {
                                    subObj.Amount = 0.0;
                                } else {
                                    var totalinvoiceamt = parseFloat(subObj.Amount).toFixed(2);
                                    var splitinvoiceamt = totalinvoiceamt.toString().split(".");
                                    splitinvoiceamt[0] = splitinvoiceamt[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                                    subObj.Amount = splitinvoiceamt.join(".");
                                }
                            }
                            htm += '<tr>';
                            htm += '<td>' + subObj.Description + '</td>';
                            htm += '<td>' + subObj.Amount + '</td>';
                            htm += '</tr>';
                        }
                    }
                    $("#InvoiceListDetails").find("tr:gt(0)").remove();
                    $("#InvoiceListDetails tbody").append(htm);
                }


                if (data.TransactionType == "Payment Receipt - Amount Due") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#ContractDueDate").css("display", "none");
                    $("#ProjectTotalDueAmount").css("display", "none");
                    $("#invoice-mainTitle").html("RECEIPT");
                    $(".cls-ModelInvoiceMainTitle").html("RECEIPT");
                    $("#billTo-tile").html("Received From");
                    $("#InvoiceNumber").html("Receipt # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice1 - portal to client") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#invoice-mainTitle").html("Invoice");
                    $(".cls-ModelInvoiceMainTitle").html("Invoice");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice3 - Total platform fees") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#invoice-mainTitle").html("PROFORMA INVOICE");
                    $(".cls-ModelInvoiceMainTitle").html("PROFORMA INVOICE");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "CREDIT MEMO") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#invoice-mainTitle").html("CREDIT MEMO");
                    $(".cls-ModelInvoiceMainTitle").html("CREDIT MEMO");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "none");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);

                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                    $("#from-Title").html("Ephylink");
                }
                if (data.TransactionType == "Invoice Freelancer") {

                    if (data.FreelancerAddress == null) {
                        data.FreelancerAddress = "";
                    }

                    $("#invoiceCeated-title").css("display", "block");
                    $("#invoiceCeated-title").css("margin-left", "4%");
                    $("#invoiceCeated-title").css("margin-top", "4%");

                    $("#from-Title").html(data.FreelancerFullName);
                    $("#invoice-mainTitle").html("INVOICE");
                    $(".cls-ModelInvoiceMainTitle").html("INVOICE");
                    $("#ContractDueDate").css("display", "block");
                    $("#ProjectTotalDueAmount").css("display", "block");
                    $("#billTo-tile").html("Bill to");
                    $("#InvoiceNumber").html("Invoice # " + data.InvoiceNumber);
                    $("#from-name").html("Name :" + data.FreelancerFullName);
                    $("#from-address").html("Address : " + data.FreelancerAddress);
                    $("#from-country").css("display", "none");

                    if (data.FreelancerCountry == "GR") {
                        if (data.FreelancerTaxType != "" && data.FreelancerTaxType != null && data.FreelancerTaxType != "null") {
                            $("#from-vatId").css("display", "block");
                            $("#from-vatId").html(data.FreelancerTaxType + "ID : " + data.FreelancerTaxId);
                        }
                    }
                    else {
                        $("#from-vatId").css("display", "none");
                    }
                }
                if (data.TransactionType == "Invoice Commision") {
                    $("#invoiceCeated-title").css("display", "none");
                    $("#from-country").css("display", "block");
                    $("#from-name").html("Dimitrios Vamvakas");
                    $("#from-address").html(" Aristofanous 5, Patras, 26500");
                    $("#from-country").css("Achaia, Greece");
                    $("#from-Title").html("Ephylink");
                    $("#from-vatId").css("display", "block");
                    $("#from-vatId").html("VAT ID: 148366653");
                }

                if (data.ClientCountry == "GR") {
                    $("#TaxDetails").css("display", "block");
                    if (data.TaxType != "" && data.TaxType != null && data.TaxType != "null") {
                        $("#TaxDetails").html(data.TaxType + " ID : " + data.TaxId)
                    }
                } else {
                    $("#TaxDetails").css("display", "none");
                }
            }

            $("#from-Title").css("font-size", "19px");
            $("#from-Title").css("font-weight", "bold");
            $("#ContractClientName").css("font-size", "19px");
            $("#ContractClientName").css("font-weight", "bold");

            $("#InvoiceId").val(invoiceId);
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result);
        }
    });
}



function GetActiveProjectDetails(Id, ServiceId, SolutionId, IndustryId) {
    //alert(Id,ServiceId,SolutionId,IndustryId);
    sessionStorage.setItem("SolutionFundId", Id)
    window.location.href = '/LandingPage/ActiveProject?Service=' + ServiceId + '&Solution=' + SolutionId + '&Industry=' + IndustryId;
}

function bindSolutionTeamFeedbackModal(data) {
    $('#FreelancerID').val(data.FreelancerId);
    $('#feedbackMessage').val(data.Feedback_Message).prop('disabled', 'disabled');
    $('input[name=communication-rating][value=' + data.CommunicationRating + ']').prop('checked', 'checked');
    $('input[name=collaboration-rating][value=' + data.CollaborationRating + ']').prop('checked', 'checked');
    $('input[name=Professionalism-rating][value=' + data.ProfessionalismRating + ']').prop('checked', 'checked');
    $('input[name=technical-rating][value=' + data.TechnicalRating + ']').prop('checked', 'checked');
    $('input[name=satisfaction-rating][value=' + data.SatisfactionRating + ']').prop('checked', 'checked');
    $('input[name=responsiveness-rating][value=' + data.ResponsivenessRating + ']').prop('checked', 'checked');
    $('input[name=likeToWork-rating][value=' + data.LikeToWorkRating + ']').prop('checked', 'checked');
    $('.freelancerFeedback_btn').prop('disabled', 'disabled');
    $('#freelancerReviewModalTitle').html('View Feedback');

    $('.rating__input').prop('disabled', 'disabled');
}

function closeFreelancerFeedbackPopup() {
    $('#freelancerReviewListModal').modal('show');
    $('#FreelancerFeedbackModal').modal('hide');
}

function resetFreelancerFeedbackForm() {
    $('#feedbackMessage').val('');
    $('input[name=communication-rating][value="0"]').prop('checked', true);
    $('input[name=collaboration-rating][value="0"]').prop('checked', true);
    $('input[name=Professionalism-rating][value="0"]').prop('checked', true);
    $('input[name=technical-rating][value="0"]').prop('checked', true);
    $('input[name=satisfaction-rating][value="0"]').prop('checked', true);
    $('input[name=responsiveness-rating][value="0"]').prop('checked', true);
    $('input[name=likeToWork-rating][value="0"]').prop('checked', true);
    $('.freelancerFeedback_btn').prop('disabled', false);
    $('#feedbackMessage').prop('disabled', false);
    $('#freelancerReviewModalTitle').html('Add Your Feedback');
    $('.rating__input').prop('disabled', false);
}

function resetSolutionFeedbackForm() {
    $('#Message').val('');
    $('input[name=wellDefined-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=adherence-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=deliverablesQuality-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=MeetingTimeliness-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=clientsatisfaction-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=adherenceToBudget-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('input[name=likeToRecommend-rating][value="0"]').prop('checked', true).prop('disabled', false);
    $('#SolutionReviewModalTitle').html('Add Your Feedback');
    $('#Message').prop('disabled', false);
    $('.SolutionFeedback_btn').prop('disabled', false);
    $('.rating__input').prop('disabled', false);
}

function openFreelancerFeedbackPopup(freelancerName, freelancerLevel, FreelancerID, IndustryID, SolutionID, ServiceId) {
    $('#freelancerReviewListModal').modal('hide');
    resetFreelancerFeedbackForm();
    $('#Industry_Id').val(IndustryID);
    $('#Solution_Id').val(SolutionID);
    $('#Service_Id').val(ServiceId);

    var data = {
        FreelancerId: FreelancerID,
        SolutionId: SolutionID,
        IndustryId: IndustryID
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: '/Home/checkSolutionTeamReviewExists',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data),
        success: function (result) {
            $('#preloader').hide();
            if (result.StatusCode == 200) {
                bindSolutionTeamFeedbackModal(result.Result);
                $('#FreelancerFeedbackModal').modal('show');
            } else {
                $('#FreelancerFeedbackModal').modal('show');
            }
        },
        error: function (result) {
            $('#preloader').hide();
            //showToaster("success", "Success", "Review submitted successfully !");
            showToaster("error", "Error !", "Failed to submit !!");
        }
    });
    $('#preloader').hide();

    $('.freelancer-name').html(freelancerName + " (" + freelancerLevel + ")");
    $('#FreelancerID').val(FreelancerID);
}

function OpenSolutionTeamPopup(ID, ServiceID, SolutionID, IndusryID) {
    $('#freelancerReviewListModal').modal('show');
    bindteamList(ID, ServiceID, SolutionID, IndusryID);
}

function bindteamList(ID, ServiceID, SolutionID, IndusryID) {
    $("#preloader").show();

    var solutionfundId = ID;
    var Industryid = IndusryID;
    var Serviceid = ServiceID;
    var Solutionid = SolutionID;


    var data = {
        IndustryId: Industryid,
        SolutionId: Solutionid,
        SolutionFundId: solutionfundId
    }
    $.ajax({
        type: "POST",
        url: "/Home/GetActiveProjectDetails",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            var data = result.Result
            var projecttype = data.SolutionFund.ProjectType;

            if (data.SolutionTeam != "") {
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = data.SolutionTeam.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = data.SolutionTeam[index];

                        htm += '<tr>';
                        htm += '<td><a onclick="openFreelancerFeedbackPopup(' + "'" + subObj.FreelancerName + "'," + "'" + subObj.FreelancerLevel + "'," +"'" + subObj.FreelancerId + "'," + Industryid + "," + Solutionid + "," + Serviceid + ');" class="feedback-link">' + subObj.FreelancerName + '</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#solutionList").find("tr:gt(0)").remove();
                $("#solutionList tbody").append(htm);
                $(".cls-temp").remove();
            }
            else {
                $(".cls-temp").remove();
                //$( "<p class='cls-temp'>No Data Available</p>" ).insertAfter("#projectReviewCarousel");
            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", "Something went wrong !!");
        }
    });
}

function OpenSolutionPopup(ID, ServiceID, SolutionID, IndustryID) {
    resetSolutionFeedbackForm();
    $('#SolutionID').val(SolutionID);
    $('#IndustryID').val(IndustryID);
    var data = {
        SolutionId: SolutionID,
        IndustryId: IndustryID
    };
    $('#preloader').show();
    $.ajax({
        type: 'POST',
        url: '/Home/checkSolutionReviewExists',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data),
        success: function (result) {
            $('#preloader').hide();
            if (result.StatusCode == 200) {
                bindSolutionFeedbackModal(result.Result);
                $('#SolutionFeedbackModal').modal('show');
            } else {
                $('#SolutionFeedbackModal').modal('show');
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

function bindSolutionFeedbackModal(data) {
    $('#SolutionID').val(data.SolutionId);
    $('#IndustryID').val(data.IndustryId);
    $('#Message').val(data.Feedback_Message).prop('disabled', 'disabled');
    $('input[name=wellDefined-rating][value=' + data.WellDefinedProjectScope + ']').prop('checked', 'checked');
    $('input[name=adherence-rating][value=' + data.AdherenceToProjectScope + ']').prop('checked', 'checked');
    $('input[name=deliverablesQuality-rating][value=' + data.DeliverablesQuality + ']').prop('checked', 'checked');
    $('input[name=MeetingTimeliness-rating][value=' + data.MeetingTimeliness + ']').prop('checked', 'checked');
    $('input[name=clientsatisfaction-rating][value=' + data.Clientsatisfaction + ']').prop('checked', 'checked');
    $('input[name=adherenceToBudget-rating][value=' + data.AdherenceToBudget + ']').prop('checked', 'checked');
    $('input[name=likeToRecommend-rating][value=' + data.LikeToRecommend + ']').prop('checked', 'checked');
    $('#SolutionReviewModalTitle').html('View Feedback');
    $('.SolutionFeedback_btn').prop('disabled', 'disabled');

    $('.rating__input').prop('disabled', 'disabled');
}

function GetFreelancerActiveProjectList() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetFreelancerActiveProjectList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result != "") {
                var Activedata = result.Result
                BindActiveProject(Activedata);
                $(".cls-temp").remove();
            }
            else {
                $(".cls-temp").remove();
                $("<p class='cls-temp'>No Data Available</p>").insertAfter("#activeProjectdiv");
            }

            $("#preloader").hide();

        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function GetClientActiveProjectList() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetActiveProjectList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result != "") {
                var Activedata = result.Result
                BindActiveProject(Activedata);
                $(".cls-temp").remove();
            }
            else {
                $(".cls-temp").remove();
                $("<p class='cls-temp'>No Data Available</p>").insertAfter("#activeProjectdiv");
            }

            $("#preloader").hide();

        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function BindActiveProject(Activedata) {
    var activeSection = "";
    $.each(Activedata, function (data, value) {
        var title = value.Title.length > 30 ? value.Title.substring(0, 30) + "..." : value.Title;
        var description = value.Description.length > 100 ? value.Description.substring(0, 100) + "..." : value.Description;
        var industryList = Activedata[data].Industries.split(",")
        var count = 0;
        //var client = "@Context.Session.GetString('LoggedUser')";

        activeSection += '<div class="col-md-6 col-xl-4 cls-activediv" style="width:250px;cursor:pointer">' +
            ' <div class="card shadow-lg cls-solutioncard">' +
            '<div class="card-body" onclick=GetActiveProjectDetails(' + value.Id + ',' + value.ServiceId + ',' + value.SolutionId + ',' + value.IndustryId + ')>' +
            '<div class="card-img justify-content-lg-center">' +
            '<img src=' + value.ImageUrlWithSas + ' alt = "SolutionImage" class="img-fluid" data-aos="zoom-out">' +
            '</div>' +
            '<h3>' +
            '<a class="cls-saveproject-title" title="' + value.Title + '">' + title + '</a>' +
            '</h3>' +
            '<p class="cls-saveProject-description" title="' + value.Description + '">' + description + '</p>' +
            '<div class="icon-image cls-industryIcon">' +
            '<img src="/assets/img/industryicon.svg" />' +
            '<label> ' + value.Industries + ' </label>' +

            '</div></div>' +
            '<div class="card-footer cls-active-footer">' +
            '<div class="d-flex align-items-center chat-message-icon position-static">' +
            "<div><i class=\"bi bi-chat-left-dots cls-chat-icon\" onclick=\"openChatModal(" + value.IndustryId + "," + value.SolutionId + ",'" + value.ClientId + "','" + value.Id + "');\"></i></div>" +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div></div>';

    })

    $('#activeProjectdiv').html(activeSection);
}

function OpenSavedProjectModalPop() {
    $("#SavedProjectModal").modal('show');
    GetAllSavedProject();
}

function GetAllSavedProject() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetAllSavedProjectList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            var data = result.Result;
            $('#SavedProjectTable').DataTable({
                destroy: true,
                data: data,
                "columns": [
                    {
                        "data": "Title",
                        "render": function (data, type, row) {
                            var title = data.length > 35 ? data.substring(0, 35) + "..." : data;
                            return "<a onclick='GotoProjectPage(" + row.ServiceId + "," + row.SolutionId + "," + row.IndustryId + ")'><span title='" + data + "'>" + title + "</span></a>";
                        }
                    },

                    {
                        "data": "Description",
                        "render": function (data, type, row) {
                            var description = data.length > 35 ? data.substring(0, 35) + "..." : data;
                            return "<span title='" + data + "'>" + description + "</span>";
                        }
                    },
                    {
                        "data": "ImageUrlWithSas",
                        "render": function (data, type, row) {
                            return '<img src="' + data + '" alt="Image" height="70" width="150" />';
                        }
                    },
                    { "data": "Services" },
                    { "data": "Industries" },
                    {
                        "data": "SolutionId",
                        "render": function (data, type, row) {
                            return '<i class="bi bi-heart-fill solution-saved" onclick="UnSavedSolutionProject(' + row.SolutionId + ',' + row.IndustryId + ')" style="color: var(--brand);cursor:pointer;"> </i>'
                        }
                    }

                ]
            });
            $("#preloader").hide();

        },
        error: function (result) {
            showToaster("error", "Error !", result);
            $("#preloader").hide();
        }
    });
}

function CloseSavedprojectPopUp() {
    $("#SavedProjectModal").modal('hide');
}

function UnSavedSolutionProject(solutionId, industryId) {
    var data = {
        SolutionId: solutionId,
        IndustryId: industryId
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/UnSavedProject",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            showToaster("success", "Success !", result.Message);
            GetSavedProjects();
            GetAllSavedProject();
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result);
        }
    });
}

function GetSavedProjects() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetSavedProjectList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            $(".cls-projectdiv").remove();
            if (result.Result != 0) {
                var data = result.Result
                BindSavedProject(data);
            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
        }
    });
}

function BindSavedProject(projectData) {
    var savedSection = "";
    $.each(projectData, function (data, value) {
        var title = value.Title.length > 30 ? value.Title.substring(0, 30) + "..." : value.Title;
        var description = value.Description.length > 100 ? value.Description.substring(0, 100) + "..." : value.Description;
        //var industryList = projectData[data].Industries.split(",")
        var count = 0;

        savedSection += '<div class="col-md-6 col-xl-4 cls-projectdiv" style="width:300px;">' +
            ' <div class="card shadow-lg cls-solutioncard">' +
            '<div class="card-body" onclick=GotoProjectPage(' + value.ServiceId + ',' + value.Id + ',' + value.IndustryId + ') style=cursor:pointer;>' +
            '<div class="card-img justify-content-lg-center">' +
            '<img src=' + value.ImageUrlWithSas + ' alt = "SolutionImage" class="img-fluid" data-aos="zoom-out">' +
            '</div>' +
            '<h3>' +
            '<a class="cls-saveproject-title" title="' + value.Title + '">' + title + '</a>' +
            '</h3>' +
            '<p class="cls-saveProject-description" title="' + value.Description + '">' + description + '</p>' +
            '<div class="icon-image cls-industryIcon">' +
            '<img src="/assets/img/industryicon.svg" />' +
            '<label> ' + value.Industries + ' </label>' +

            '</div></div>' +
            '<div class="card-footer">' +
            '<div class="like cls-heart-icon">' +
            '<i class="bi bi-heart-fill solution-saved" onclick=UnSavedSolutionProject(' + value.Id + ',' + value.IndustryId + ') style="color: var(--brand);cursor:pointer;"> </i>' +
            '</div>' +
            '</div>' +
            '</div></div>';


    })
    $(savedSection).insertBefore("#favorites-card");
}

//function OpenDetailsPopModal(Id, SolutionId, IndustryId) {
//    $("#DetailsPopModal").modal('show');
//    $("#SolutionId").val(SolutionId);
//    $("#IndustryId").val(IndustryId);
//    GetMiletoneList();
//    GetPointsList();
//    GetSolutionDefineDetails();
//}




//function GetSolutionDefineDetails() {

//    var data = {
//        SolutionId: parseInt($("#SolutionId").val()),
//        IndustryId: parseInt($("#IndustryId").val()),
//        ProjectType: $("#ProjectType").val()
//    };

//    $("#preloader").show();

//    $.ajax({
//        type: "POST",
//        url: "/Home/GetSolutionDefineData",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify(data),
//        success: function (result) {
//            if (result.StatusCode == 200) {
//                $("#industry-ProjectDetails").val(result.Result.ProjectDetails);
//                $("#industry-ProjectOutline").val(result.Result.ProjectOutline);
//                $("#industry-ProjectDuration").val(result.Result.Duration)
//                GetMiletoneList();
//                GetPointsList();
//            }
//            else {
//                showToaster("error", "Error !", "Failure");
//            }
//            var ProjectType = $("#ProjectType").val();
//            if (ProjectType == "LARGE") {
//                $("#industry-TeamSize").val(5);
//            }
//            if (ProjectType == "MEDIUM") {
//                $("#industry-TeamSize").val(3);
//            }
//            if (ProjectType == "SMALL") {
//                $("#industry-TeamSize").val(2);
//            }
//        },
//        error: function (result) {
//            //alert("failure");
//            showToaster("error", "Error !", result);
//            $("#preloader").hide();
//        }
//    });
//}

function getFileData(myFile) {
    $("#ChangeCvUpload").val(false)
    $('#filePreview').css("display", "none");
    var file = myFile.files[0];
    if (file) {
        var filename = file.name;
        $('#filePreview').css("display", "block");
        $('#filePreview').val(filename);
    }
    else {
        $('#filePreview').css("display", "none");
        $('#filePreview').val("");
    }
}

//function SaveSolutionIndustryDescription() {
//    var data = $("#industry-ProjectOutline").val();
//    if (data == "") {
//        return
//    }

//    var DefineData = {
//        Id: $("#ProjectOulineId").val(),
//        ProjectOutline: $("#industry-ProjectOutline").val(),
//        ProjectDetails: $("#industry-ProjectDetails").val(),
//        SolutionId: $("#SolutionId").val(),
//        IndustryId: $("#IndustryId").val(),
//        ProjectType: $("#ProjectType").val(),
//        Duration: $("#industry-ProjectDuration").val(),
//        TeamSize: $("#industry-TeamSize").val(),
//    };

//    $("#preloader").show();
//    $.ajax({
//        type: "POST",
//        url: "/Home/UpdateIndustryOutline",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify(DefineData),
//        success: function (result) {
//            //alert(result.Message);
//            showToaster("success", "Success", result.Message);
//            ResetDetailsForm()
//            $("#preloader").hide();
//        },
//        error: function (result) {
//            //alert("Error occured..");
//            showToaster("error", "Error !", result);
//        }
//    });
//}

//function ResetDetailsForm() {
//    $("#industry-ProjectOutline").val("")
//    $("#industry-ProjectDetails").val("")
//    $("#SolutionId").val(0)
//    $("#IndustryId").val(0)
//    $("#ProjectType").val("SMALL")
//    ResetMileStoneForm()
//    $("#DetailsPopModal").modal('hide')
//}

//function hideGuidelinesModal() {
//    $('#guidelineModal').modal('hide');
//    $("#DetailsPopModal").modal('show');
//}
//function showGuidelinesModal() {
//    $('#guidelineModal').modal('show');
//}

function BindProjects() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetDashbordContractUser",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Message == "success") {
                $("#ContractUser-Value").text(result.Result);
            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", "Failure");
        }
    });
}

function ViewInvoiceGridPopUp(contractId) {
    //
    //$("#GridContractId").val(contractId)
    var data = {
        ContractId: contractId
    };

    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetInvoiceTranscationTypeDetails",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            $("#InvoiceGridPopUpModal").modal('show');
            if (result.Result.length != 0) {
                $("#InvoiceGridTable").show();
                var invoiceData = result.Result;
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = invoiceData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = invoiceData[index];
                        htm += '<tr>';
                        htm += '<td>' + subObj.InvoiceName + '</td>';
                        htm += '<td><a class="text-primary" onclick=OpenInvoiceModalPopUp(' + subObj.Id + ')>View</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#InvoiceGridTable").find("tr:gt(0)").remove();
                $("#InvoiceGridTable tbody").append(htm);
                $(".invoice-temp").hide();
            }
            else {
                $("#InvoiceGridTable").hide()
                $("<p class='invoice-temp'>No Data Available</p>").insertAfter("#InvoiceGridTable");

            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
        }
    });
}

function ViewFreelancerInvoiceGridPopUp(contractId) {
    //
    //$("#GridContractId").val(contractId)
    var data = {
        ContractId: contractId
    };

    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetFreelancerInvoiceTypeDetails",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            $("#InvoiceGridPopUpModal").modal('show');
            if (result.Result.length != 0) {
                $("#InvoiceGridTable").show();
                var invoiceData = result.Result;
                var index = 0;
                var subObj = '';
                var htm = '';
                var resultLength = invoiceData.length;
                if (resultLength > 0) {
                    for (index = 0; index < resultLength; index++) {
                        subObj = invoiceData[index];
                        htm += '<tr>';
                        htm += '<td>' + subObj.InvoiceName + '</td>';
                        htm += '<td><a class="text-primary" onclick=OpenFreelancerInvoiceModalPopUp(' + subObj.Id + ')>View</a></td>';
                        htm += '</tr>';
                    }
                }
                $("#InvoiceGridTable").find("tr:gt(0)").remove();
                $("#InvoiceGridTable tbody").append(htm);
                $(".invoice-temp").hide();
            }
            else {
                $("#InvoiceGridTable").hide()
                $("<p class='invoice-temp'>No Data Available</p>").insertAfter("#InvoiceGridTable");

            }
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result);
        }
    });
}

function CloseInvoiceGridPopUp() {
    $("#InvoiceGridPopUpModal").modal('hide');
}

function CloseSolutionPopup() {
    $('#SolutionFeedbackModal').modal('hide');
}

function CloseSolutionTeamPopup() {
    $('#freelancerReviewListModal').modal('hide');
    $(".cls-solutionListBody").empty();
}

function CloseInvoicePopUp() {
    $("#InvoiceModal").modal('hide')
}

function OpenCreditMemoModalPopUp(ContractId) {
    $("#CreditMemoModal").modal('show');
}

function CloseCreditMemoModalPopUp() {
    $("#CreditMemoModal").modal('hide');
}

function DownloadCreditMemoInvoice() {
    $("#preloader").show();
    var contractId = $("#InvoiceContractId").val();
    window.location = "/Home/DownloadCreditMemo";

    // $("#preloader").hide();
    setTimeout(function () {
        $("#preloader").hide();
    }, 10000);
}

function CloseGigOpeningApplyModal() {
    $("#GigOpeningApplyModal").modal('hide')
}
function clearApplyForm() {
    $("#Description").val("");
    $("#CVPath").val("");
    $('#filePreview').css("display", "none");
}

function getOpenRoles() {
    $("#preloader").show();
    $.ajax({
        type: "Get",
        url: "/Home/GetRolesList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {

            var index = 0;
            var subObj = '';
            var htm = '';
            for (index = 0; index < result.Result.length; index++) {
                if (index <= 9) {
                    subObj = result.Result[index];

                    htm += '<tr>';
                    htm += '<td class="cls-RolesId" >' + subObj.ID + '</td>';
                    htm += '<td>' + subObj.ServiceName + '</td>';
                    htm += '<td>' + subObj.SolutionName + '</td>';
                    htm += '<td>' + subObj.Title + '</td>';
                    htm += '<td>' + subObj.Level + '</td>';
                    htm += '<td>' + subObj.IndustryName + '</td>';
                    htm += '<td><a class="btn btn-success applyFor-btn pt-0 pb-1 btn-sm" style="color: white;" onclick="GigApply(' + subObj.ID + ')">View Details</a></td>';
                    htm += '</tr>';
                }
            }
            $("#table-OpenRoles").find("tr:gt(0)").remove();
            $("#table-OpenRoles tbody").append(htm);
            makeTableScroll();
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", "Failure");
            $("#preloader").hide();
        }
    });

    //getOpenRoles();
    //$("#ProjectType").change(function () {
    //    GetSolutionDefineDetails()
    //});
}

