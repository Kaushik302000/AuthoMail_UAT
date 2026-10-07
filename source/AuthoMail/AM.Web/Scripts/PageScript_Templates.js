var DolfusTemplates = function () {
    var gridOptions;

    var calcDataTableHeight = function () {
        var decreaseHeight = (($(window).innerHeight() * 190) / 657);
        return ($(window).innerHeight() - decreaseHeight);
    };
    function onQuickFilterChanged(gridOptions) {
        $("#quickFilter").on("input", function (e) {
            if (e.which == 13) {
                return false;
            }
            gridOptions.api.setQuickFilter(document.getElementById('quickFilter').value);
            //console.log(gridOptions.api.getDisplayedRowCount())
            $("#filterLength").text('Showing ' + gridOptions.api.getDisplayedRowCount() + ' of ' + $("#totalcounts").text());

        });
    }
    function ApplyAGGrid() {
        $("#requestGrid").empty();
        var columnDefs = [
            { headerName: 'Name', field: 'FileName', sortable: true, cellStyle: { 'text-align': 'left !important' }, headerClass: 'leftalign' },
            { headerName: 'Size (KB)', field: 'FileSize', sortable: true, },
            {
                headerName: 'Modified On', field: 'ModifiedOn', sortable: true, sort: 'desc', cellRenderer: function (params) {
                    return moment(params.value).format("MM/DD/YY HH:mm:ss");
                }, getQuickFilterText: function (params) {
                    return moment(params.value).format("MM/DD/YY HH:mm:ss");
                }
            },
            {
                headerName: 'Action', field: 'FileName', sortable: false, cellRenderer: function (params) {
                    if (params.data.Status == true) {

                        var fileNameWithOutExt = params.data.FileName.substring(0, params.data.FileName.lastIndexOf('.'));
                        var fileExt = params.data.FileName.substring(params.data.FileName.lastIndexOf('.'));
                        var buttons = '<button title="Download Template" type="button" class="btn btn-success btn-sm downloadtemplate" data-id="' + params.data.FileName + '"><i class="fa fa-download"></i></button>';
                        if (params.data.IsLargeTemplateExist) {
                            buttons += ' <button type="button" title="Download Large Font Template" class="btn btn-primary btn-sm largefonttemplate" data-id="' + fileNameWithOutExt + "_16PT" + fileExt + '"><i class="fa fa-arrow-circle-down"></i></button>';
                        }
                        buttons += ' <button type="button" title="View Template History" class="btn btn-info btn-sm historytemplate" data-id="' + params.data.FileName + '"><i class="fa fa-history"></i></button>';
                        buttons += ' <button type="button" title="Delete Template" class="btn btn-danger btn-sm removetemplate" data-id="' + params.data.FileName + '"><i class="fa fa-trash"></i></button>';
                        var eDiv = document.createElement('span');
                        eDiv.innerHTML = buttons;
                        var historytemplate = eDiv.querySelector('.historytemplate');
                        historytemplate.addEventListener('click', function () {
                            GetHistory($(this).data('id'));
                        });
                        var downloadtemplate = eDiv.querySelector('.downloadtemplate');
                        downloadtemplate.addEventListener('click', function () {
                            DownloadTemplate($(this));
                        });
                        var largefonttemplate = eDiv.querySelector('.largefonttemplate');
                        if (largefonttemplate !== null) {
                            largefonttemplate.addEventListener('click', function () {
                                DownloadTemplate($(this));
                            });
                        }
                        var removetemplate = eDiv.querySelector('.removetemplate');
                        removetemplate.addEventListener('click', function () {
                            RemoveTemplate($(this), gridOptions);
                        });

                        return eDiv;
                    } else {
                        return '--';
                    }
                }

            }
        ];

        fetch(baseUrl + "DelfusTemplate/DelfusTemplatesList")
            .then(response => response.json())
            .then((data) => {
                $("#quickFilter").val("");
                $("#totalcounts").text(data.data.length);
                $("#filterLength").text('Showing ' + data.data.length + ' of ' + data.data.length);
                gridOptions = {
                    defaultColDef: {
                        sortable: true,
                        filter: false,
                        resizable: true,
                        cellClass: 'cell-copy-text',
                    },
                    columnDefs: columnDefs,
                    //rowSelection: 'single',
                    //rowDeselection: true,
                    suppressCellSelection: true,
                    rowData: data.data,
                    //floatingFilter: true,
                    //onCellClicked: (e) => {
                    //    if (e.column.colId !== 'bookingRefNo') { // cell is from non-select column
                    //        e.node.setSelected(true);
                    //    }
                    //},
                    //onSelectionChanged: onSelectionChanged,
                    //onGridSizeChanged: onGridSizeChanged,
                    onGridReady: function (params) {
                        params.api.sizeColumnsToFit();
                        $("#loading").hide();
                        window.addEventListener('resize', function () {
                            setTimeout(function () {
                                params.api.sizeColumnsToFit();
                            });
                        });

                    },
                };
                // gridOptions.api.setRowData(data);
                var gridDiv = document.querySelector('#requestGrid');
                console.log(gridOptions, gridDiv)
                new agGrid.Grid(gridDiv, gridOptions);
                gridOptions.api.sizeColumnsToFit();
                onQuickFilterChanged(gridOptions);
                $("#requestGrid").height(calcDataTableHeight());
            });
    }
    function DropZoneInit() {
        var uploadedProgress = 0;
        var totalError = 0;
        var dropzoneObj;
        var uploadedFile = 0;
        var dropedFile = 0;
        Dropzone.options.uploadtemplatediv = {
            paramName: "template", // The name that will be used to transfer the file
            //maxFilesize: 2, // MB
            //timeout: 60000,
            //uploadMultiple:true,
            //parallelUploads:100,
            dictDefaultMessage: '',//<b>Click or Drag here to upload template(s).</b>',
            //acceptedFiles:  'file/doc, file/docx',//'doc, docx',//"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            //accept: function (file, done) {
            //    //console.log('file:', file);
            //    done();
            //},
            autoQueue: true,
            autoDiscover: false,
            uploadprogress: function (file, progress, bytesSent) {
                //$("#progressBar").show();
            },
            //processing: function (response) {
            //    console.log('processing: ', response);
            //    $("#progressBar").show();
            //},

            sending: function (response) {
                //console.log('sending: ', response.name);
                $("#progressBar").show();
            },
            //success: function (file,response) {
            //    console.log('success : ', file.name);
            //    console.log('success : ', response);
            //},
            //complete: function (response) {

            //},
            totaluploadprogress: function (response) {
                //uploadedProgress = response >= uploadedProgress ? response : uploadedProgress;
                uploadedProgress = parseInt(uploadedFile * 100) / dropzoneObj.files.length;
                // console.log(parseInt(uploadedFile / dropzoneObj.files.length))
                $("#progressBar").val(Math.round(uploadedProgress));
                //console.log('totaluploadprogress:', response, uploadedFile, uploadedProgress, dropzoneObj);
                //alertMessage("green", 'All templates uploaded successfully.');
            },
            queuecomplete: function (response) {
                //console.log(response);


                $("#progressBar").hide();
                //console.log(dropzoneObj.files.length, totalError)
                if (totalError > 0) {
                    if (dropedFile == totalError) {
                        alertMessage("red", 'Upload failed with ' + totalError + ' error(s). You are being redirected to “Upload summary” page for more details...');

                    }
                    else {
                        alertMessage("yellow", 'Upload completed with ' + totalError + ' error(s). You are being redirected to “Upload summary” page for more details...');
                        setTimeout(function () {
                            ApplyAGGrid();
                        }, 1000);
                    }
                    setTimeout(function () {
                        window.open(baseUrl + 'DelfusTemplate/TemplateSummary');
                    }, 4000);
                    totalError = 0;
                    uploadedFile = 0;
                } else {
                    alertMessage("green", 'Upload completed: Please check upload summary for more details.');
                    ApplyAGGrid();
                }
                dropedFile = 0;
                //window.open(baseUrl + 'DelfusTemplate/TemplateSummary');
            },
            error: function (err) {
                //console.log('error: ', err);
            },

            init: function () {
                dropzoneObj = this;
                //this.on("addedfile", function (file) {
                //    console.log('addedfile: ', file);
                //});
                this.on("success", function (file, response) {
                    var table = $('#templatelisttable').DataTable();
                    if (response.Success) {
                        uploadedFile += 1;


                    } else {
                        totalError += 1;
                    }
                    dropedFile += 1;
                });
                this.on("error", function (file, errorMessage) {
                    //console.log('err: ', errorMessage);
                    totalError += 1;
                    dropedFile += 1;
                });
            }
        };

    }
    function calcDataTableHeightWithParam(decreaseTableHeight) {
        return ($(window).innerHeight() - 240) - decreaseTableHeight;
    };
    function RemoveTemplate(obj) {
        var fileName = $(obj).data('id');
        var isConfirm = confirm('Are you sure you want to delete "' + fileName + '" template?');
        if (isConfirm) {
            var context = $(obj);
            $(obj).attr('disabled', 'disabled');
            $.ajax({
                type: "POST",
                url: baseUrl + "DelfusTemplate/RemoveDelfusTemplate",
                data: { fileName: fileName },
                dataType: "json",
                success: function (response) {
                    if (response.Success) {
                        alertMessage("green", 'Template has been deleted.');
                    }
                    else {
                        alertMessage("red", response.ErrorMessage);
                    }
                    $(context).removeAttr('disabled');
                    ApplyAGGrid();
                },
                error: function (err) {
                    console.log(err);
                    $(context).removeAttr('disabled');
                }
            });
        }
    }
    function GetHistory(fileName) {
        $("#historyModal").modal('show');
        $("#historyLabel").text('History of ' + fileName);
        var url = baseUrl + "DelfusTemplate/DelfusTemplatesHistory?fileName=" + fileName;
        var columnDefs = [
            {
                headerName: 'User Action', field: 'Status', sortable: true, cellRenderer: function (params) {
                    if (params.value == "Modified") {
                        return '<span class="label label-success"> Modified </span>';
                    }
                    else if (params.value == "Deleted" || params.value == "Invalid") {
                        return '<span class="label label-danger"> ' + params.value + ' </span>';
                    }
                    else if (params.value == "Added") {
                        return '<span class="label label-info"> Added </span>';
                    }
                    else {
                        return '--';
                    }
                }
            },
            { headerName: 'Modified By', field: 'ModifiedBy', sortable: true, },
            {
                headerName: 'Modified On', field: 'ModifiedOn', sortable: true, sort: 'desc', cellRenderer: function (params) {
                    return moment(params.value).format("MM/DD/YY HH:mm:ss");
                }
            },
        ];
        var historyGridOptions;
        fetch(url)
            .then(response => response.json())
            .then((res) => {
                console.log(res.data);
                historyGridOptions = {
                    defaultColDef: {
                        sortable: true,
                        filter: false,
                        resizable: true
                    },
                    columnDefs: columnDefs,
                    //rowSelection: 'single',
                    //rowDeselection: true,
                    suppressCellSelection: true,
                    rowData: res.data,
                    //floatingFilter: true,
                    //onCellClicked: (e) => {
                    //    if (e.column.colId !== 'bookingRefNo') { // cell is from non-select column
                    //        e.node.setSelected(true);
                    //    }
                    //},
                    //onSelectionChanged: onSelectionChanged,
                    //onGridSizeChanged: onGridSizeChanged,
                    onGridReady: function (params) {
                        params.api.sizeColumnsToFit();

                        window.addEventListener('resize', function () {
                            setTimeout(function () {
                                params.api.sizeColumnsToFit();
                            });
                        });

                    },
                };
                // gridOptions.api.setRowData(data);
                var gridDiv = document.querySelector('#historyGrid');
                console.log(historyGridOptions, gridDiv)
                $('#historyGrid').empty();
                new agGrid.Grid(gridDiv, historyGridOptions);
                historyGridOptions.api.sizeColumnsToFit();
                onQuickFilterChanged(historyGridOptions);
                $("#historyGrid").height(calcDataTableHeightWithParam(70));

            });
        $("#historyModal").modal('show');
    }
    function DownloadTemplate(obj) {
        var context = $(obj);
        $(obj).attr('disabled', 'disabled');
        $(obj).find('i').removeClass('fa-download');
        $(obj).find('i').addClass('fa-spining fa-spinner fa-spin');
        var fileName = $(obj).data('id');
        $.ajax({
            type: "POST",
            url: baseUrl + "DelfusTemplate/DownloadDelfusTemplate",
            data: { fileName: fileName },
            dataType: "json",
            success: function (response) {
                //console.log(response);
                if (response.Success) {
                    var stream = response.Object.stream;
                    var sampleArr = base64ToArrayBuffer(stream);
                    saveByteArray(response.Object.fileName, sampleArr);
                }
                else {
                    alertMessage("red", response.ErrorMessage);
                }
                $(context).removeAttr('disabled');
                $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                $(context).find('i').addClass('fa-download');
            },
            error: function (err) {
                //console.log(err);
                $(context).removeAttr('disabled');
                $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                $(context).find('i').addClass('fa-download');
            }
        });
    }

    function DownloadLargeFontTemplate(obj) {
        var context = $(obj);
        $(obj).attr('disabled', 'disabled');
        $(obj).find('i').removeClass('fa-download');
        $(obj).find('i').addClass('fa-spining fa-spinner fa-spin');
        var fileName = $(obj).data('id');
        $.ajax({
            type: "POST",
            url: baseUrl + "DelfusTemplate/DownloadDelfusTemplate",
            data: { fileName: fileName },
            dataType: "json",
            success: function (response) {
                //console.log(response);
                if (response.Success) {
                    var stream = response.Object.stream;
                    var sampleArr = base64ToArrayBuffer(stream);
                    saveByteArray(response.Object.fileName, sampleArr);
                }
                else {
                    alertMessage("red", response.ErrorMessage);
                }
                $(context).removeAttr('disabled');
                $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                $(context).find('i').addClass('fa-download');
            },
            error: function (err) {
                //console.log(err);
                $(context).removeAttr('disabled');
                $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                $(context).find('i').addClass('fa-download');
            }
        });
    }

    function base64ToArrayBuffer(base64) {
        var binaryString = window.atob(base64);
        var binaryLen = binaryString.length;
        var bytes = new Uint8Array(binaryLen);
        for (var i = 0; i < binaryLen; i++) {
            var ascii = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    }

    //save Byte[] array and download
    function saveByteArray(fileName, byte) {
        var blob = new Blob([byte]);
        var link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName + '.docx';
        link.click();
    }
    return {
        init: function () {
            DropZoneInit();
            ApplyAGGrid();
        }
    };
}();
jQuery(document).ready(function () {
    DolfusTemplates.init();
    $("#loading").show();
    $(document).on("keydown", "form", function (event) {
        return event.key != "Enter";
    });
});