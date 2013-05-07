
// Contains the string ID of the currently-open sidebar panel.
var sidebarShown = null;

function layoutMapAndSidebar() {
	var winw = $(window).width();
	var sidew = $("#sidebararea").outerWidth();

	var winh = $(window).height();

	$("#map_canvas").width(winw - sidew);
	$("#sidebararea").css("left", winw-sidew);

	$("#map_canvas,#sidebar-bg").height(winh - $("#header").outerHeight());
	$("#map_canvas,#sidebar-bg,#sidebararea").css("top", $("#header").outerHeight());
	$("#sidebararea-int").height(winh - $("#header").outerHeight());
	// 15 = left padding + margin.
	$("#sidebararea-int").width($("#sidebararea").width() - 15);

	// Reset the canvas position type.
	$("#map_canvas").css("position", "fixed");

	$("#header").css("width", winw);

	// Manually trigger the google maps resize event.
	if(map)
		google.maps.event.trigger(map, 'resize');
}

$(layoutMapAndSidebar);
$(window).add("#sidebararea").resize(layoutMapAndSidebar);

function slideSidebarTo(left, onComplete) {
	$("#sidebararea").animate(
		{
			"width": left,
		},
		{
			"progress": function(now, fx) {
				layoutMapAndSidebar();
			},
			"complete": function() {
				$("#sidebar-contents").css("width", "");
				if(sidebarShown == null) {
					$("#sidebar-contents").hide();
				}
				if(onComplete) {
					onComplete();
				}
				layoutMapAndSidebar();
			}
		}
	);
}

function showOnSidebar(id, sidebarWidth) {
	// If it's open already, must close before switching which element is displayed.
	if(sidebarShown) {
		hideSidebar(function() { showOnSidebar(id, sidebarWidth); });
	} else {
		slideSidebarTo(sidebarWidth+"px");
		$("#sidebar-contents > div").hide();
		$("#"+id).show();
		$("#sidebar-contents").show();
		$("#sidebar-contents").css("width", sidebarWidth);
		sidebarShown = id;
	}
	hidePullover();
}

// Completely collapses the sidebar, hiding all.
function hideSidebar(onComplete) {
	sidebarShown = null;
	$("#sidebar-contents").width($("#sidebar-contents").width());
	slideSidebarTo("0px", function() {
		if(onComplete)
			onComplete();
	});
	$("#header .sidebar-group .btn").removeClass("active");
}

// Resizing functionality

$(function() {
	$("#sidebararea").mousedown(function(e) {
		if(e.target == this) {
			$("#sidebararea").disableSelection();

			function onMove(e) {
				var winw = $(window).width();
				$("#sidebararea").width(
					winw - e.pageX
				);
				layoutMapAndSidebar();
			}
			function letGo(e) {
				$(document).unbind('mousemove', onMove);
				$(document).unbind('mouseleave', letGo);
				$(document).unbind('mouseup', letGo);

				$("#sidebararea").enableSelection();
			}
			$(document).mousemove(onMove);
			$(document).mouseleave(letGo).mouseup(letGo);

		}
	});
});


// Suggestion stuff

function loadSuggestions(data, setUrl, defaultSetUrl) {
	console.log(data);
	datadebug = data;

	var nodesList = [];
	var linksList = [];
	var i = 0;
	for(var n in data.nodes) {
		nodesList[i++] = data.nodes[n];
	}
	i = 0;
	for(var n in data.links) {
		linksList[i++] = data.links[n];
	}

	nodesList.sort(function(a,b) {
		return b.onode.ExpansionSuggested - a.onode.ExpansionSuggested
	});
	linksList.sort(function(a,b) {
		return b.olink.ExpansionSuggested - a.olink.ExpansionSuggested
	});

	var nodeExpMax = $("#defaultNodeExpMax").val();
	var nodeExpPerUnit = $("#defaultNodeExpCostPerUnit").val();
	var linkExpMax = $("#defaultLinkExpMax").val();
	var linkExpPerUnit = $("#defaultLinkExpCostPerUnit").val();

	for(var n in nodesList) {
		var node = nodesList[n];

		var row = '<td>';
		row += node.code;
		row += '</td><td class="max-possible">';
		row += modifiableTextHtml(node.onode.Expansion.CapacityExpansionMaxPossible, nodeExpMax);
		row += '</td><td class="cost-per-unit">';
		row += modifiableTextHtml(node.onode.Expansion.CapacityExpansionCostPerUnit, nodeExpPerUnit);
		row += '</td><td style="text-align: center;"><strong>';
		row += node.onode.ExpansionSuggested;
		row += "</strong></td><td>";
		if(node.onode.ExpansionSuggested > 0)
			row += '<a href="#" class="btn small-btn implement-btn"><i class="icon-pencil"></i></a>';
		row += '</td>';

		$("#nodeSuggestionTable").append('<tr data-id="'+node.id+'">'+row+"</tr>");
	}
	for(var l in linksList) {
		var link = linksList[l];

		var row = '<td>';
		row += link.from;
		row += "</td><td>";
		row += link.to;
		row += '</td><td class="max-possible">';
		row += modifiableTextHtml(link.olink.Expansion.CapacityExpansionMaxPossible, linkExpMax);
		row += '</td><td class="cost-per-unit">';
		row += modifiableTextHtml(link.olink.Expansion.CapacityExpansionCostPerUnit, linkExpPerUnit);
		row += '</td><td style="text-align: center;"><strong>';
		row += link.olink.ExpansionSuggested;
		row += "</strong></td><td>";
		if(link.olink.ExpansionSuggested > 0)
			row += '<a href="#" class="btn small-btn implement-btn"><i class="icon-pencil"></i></a>';
		row += '</td>';

		$("#linkSuggestionTable").append('<tr data-id="'+link.id+'">'+row+"</tr>");
	}
	// Now that everything's added, set it all up.
	$(".small-input .modifiable-text").keydown(function(e) {
		setTimeout(changeSubmit, 1, e);
	});

	function changeSubmit(e) {
		var modt = $(e.target).closest(".modifiable-text");
		modt.find(".acceptValueBtn").css("display", "none");
		modt.append('<span class="spinner"><img src="/Content/img/ajax-loader.gif" /></span>');

		showOutOfDate();

		var v = modt.find(".input-mini").val().replace(/[^0-9]/g, "");
		if(v != modt.find(".input-mini").val())
			modt.find(".input-mini").val(v);
		if(v == "")
			v = -1;

		function done(retData) {
			if(retData.success == true) {
				modt.find(".spinner").remove();
				setupModifiableText(modt, data);
				$("#sidebar-reoptimize-btn").removeClass("disabled");
			} else {
				console.log(data);
			}
		}

		var node = true;
		if(modt.closest("table").attr("id") == "linkSuggestionTable") {
			node = false;
		}

		$.post(setUrl,
			{
				id: modt.closest("tr").attr("data-id"),
				maxExp: modt.closest("td").hasClass("max-possible") ? v : null,
				costPerCar: modt.closest("td").hasClass("cost-per-unit") ? v : null,
				node: node
			},
			done, "json"
		);
	}

	$(".removeValueBtn").click(function(e) {
		var modt = $(e.target).closest(".modifiable-text");
		modt.find(".removeValueBtn").css("display", "none");
		modt.find("input").val("").focus();
		changeSubmit(e);
	});

	function setDefaults(t, maxNodeExp, nodeCostPer, maxLinkExp, linkCostPer) {
		$(t).closest("td").append('<span class="spinner"><img src="/Content/img/ajax-loader.gif" /></span>');

		showOutOfDate();

		if(maxNodeExp != null) {
			maxNodeExp = maxNodeExp.val().replace(/[^0-9]/g, "")
			$("#defaultNodeExpMax").val(maxNodeExp);
		}
		if(nodeCostPer != null) {
			nodeCostPer = nodeCostPer.val().replace(/[^0-9]/g, "")
			$("#defaultNodeExpCostPerUnit").val(nodeCostPer);
		}
		if(maxLinkExp != null) {
			maxLinkExp = maxLinkExp.val().replace(/[^0-9]/g, "")
			$("#defaultLinkExpMax").val(maxLinkExp);
		}
		if(linkCostPer != null) {
			linkCostPer = linkCostPer.val().replace(/[^0-9]/g, "")
			$("#defaultLinkExpCostPerUnit").val(linkCostPer);
		}

		$.post(defaultSetUrl,
			{
				maxNodeExp: maxNodeExp,
				nodeCostPer: nodeCostPer,
				maxLinkExp: maxLinkExp,
				linkCostPer: linkCostPer,
			},
			function() {
				$(t).closest("td").find(".spinner").remove();
				$(t).closest("div").find("button").css("display", "none");
				if(maxNodeExp != null) {
					$("#nodeSuggestionTable").find(".max-possible")
						.find("input").attr("placeholder", maxNodeExp);
				}
				if(nodeCostPer != null) {
					$("#nodeSuggestionTable").find(".cost-per-unit")
						.find("input").attr("placeholder", nodeCostPer);
				}
				if(maxLinkExp != null) {
					$("#linkSuggestionTable").find(".max-possible")
						.find("input").attr("placeholder", maxLinkExp);
				}
				if(linkCostPer != null) {
					$("#linkSuggestionTable").find(".cost-per-unit")
						.find("input").attr("placeholder", linkCostPer);
				}
				$("#sidebar-reoptimize-btn").removeClass("disabled");
			}, "json"
		);
	}

	$("#defaultNodeExpMax").keydown(function(e) {		setTimeout(setDefaults, 1, e.target, $("#defaultNodeExpMax"), null, null, null);
	});
	$("#defaultNodeExpCostPerUnit").keydown(function(e) {
		setTimeout(setDefaults, 1, e.target, null, $("#defaultNodeExpCostPerUnit"), null, null);
	});
	$("#defaultLinkExpMax").keydown(function(e) {
		setTimeout(setDefaults, 1, e.target, null, null, $("#defaultLinkExpMax"), null);
	});
	$("#defaultLinkExpCostPerUnit").keydown(function(e) {
		setTimeout(setDefaults, 1, e.target, null, null, null, $("#defaultLinkExpCostPerUnit"));
	});
}

function setupModifiableText(elem, data) {
	var id = $(elem).closest("tr").attr("data-id");

	var node = true;
	if(elem.closest("table").attr("id") == "linkSuggestionTable") {
		node = false;
	}

	var opt = node ? data.nodes[id].onode : data.links[id].olink;

	var val = $(elem).find("input").val();

	if(val == "") {
		$(elem).attr("value", "");
		$(elem).find(".removeValueBtn").css("display", "none");
	} else {
		$(elem).attr("value", val);
		$(elem).find(".removeValueBtn").css("display", "inline");
	}
}

function modifiableTextHtml(curVal, placeholder) {
	var h = '<div class="input-append modifiable-text">';
	h += '<input class="input-mini" type="text" placeholder="'+placeholder;
	h += '" value="'+(curVal != null ? curVal : "")+'">';
	h += '<button style="display:none;" class="btn acceptValueBtn" type="button"><i class="icon-ok" /></button>';
	h += '<button ';
	if(curVal == null) {
		h += 'style="display:none;" ';
	}
	h += 'class="btn removeValueBtn" type="button"><i class="icon-remove" /></button>';
	h += '</div>';
	return h;
}
