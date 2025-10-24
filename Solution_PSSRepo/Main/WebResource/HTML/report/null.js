function vis_btnDelete() {
	debugger;
	var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
	if (statuscode != 1||checkRoles("PL_CNTT")) {
		return false;
	}
}
function checkRoles(roleName) {
	debugger;
	var globalContext = Xrm.Utility.getGlobalContext();
	var listRoles = globalContext.userSettings.roles;
	var hasRole = false;
	listRoles.forEach(function hasName(item, index) {
		if (item.name == roleName) {
			hasRole = true;
		};
	});
	return hasRole;
}