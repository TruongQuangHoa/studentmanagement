@using StudentManagement.Areas.Admin.Models
@model IList<AdminMenu>

<aside id="sidebar" class="sidebar">
    <ul class="sidebar-nav" id="sidebar-nav">
        @foreach (var item in Model.Where(m => m.ItemLevel == 1).OrderBy(m => m.ItemOrder))
        {
            var pID = item.AdminMenuID;
            var sMenu = Model.Where(p => p.ParentLevel == pID).OrderBy(p => p.ItemOrder).ToList(); // cấp 2
            var hasSubMenu = sMenu.Any();
            <li class="nav-item">
                <a class="nav-link @((hasSubMenu ? "collapsed" : ""))"
                   data-bs-toggle="@(hasSubMenu ? "collapse" : null)"
                   data-bs-target="#menu-@item.AdminMenuID"
                   href="@(!hasSubMenu ? $"/Admin/{item.ControllerName}/{item.ActionName}" : "#")">
                    <i class="@item.Icon"></i><span>@item.ItemName</span>
                    @if (hasSubMenu)
                    {
                        <i class="bi bi-chevron-down ms-auto"></i>
                    }
                </a>

                @if (hasSubMenu)
                {
                    <ul id="menu-@item.AdminMenuID" class="nav-content collapse">
                        @foreach (var smn in sMenu)
                        {
                            var subSubMenu = Model.Where(p => p.ParentLevel == smn.AdminMenuID).OrderBy(p => p.ItemOrder).ToList(); // cấp 3
                            var hasThirdLevel = subSubMenu.Any();
                            <li>
                                <a class="nav-link @((hasThirdLevel ? "collapsed" : ""))"
                                   data-bs-toggle="@(hasThirdLevel ? "collapse" : null)"
                                   data-bs-target="#submenu-@smn.AdminMenuID"
                                   href="@(!hasThirdLevel ? $"/Admin/{smn.ControllerName}/{smn.ActionName}" : "#")">
                                    <i class="@smn.Icon ?? "bi bi-circle""></i><span>@smn.ItemName</span>
                                    @if (hasThirdLevel)
                                    {
                                        <i class="bi bi-chevron-down ms-auto"></i>
                                    }
                                </a>

                                @if (hasThirdLevel)
                                {
                                    <ul id="submenu-@smn.AdminMenuID" class="nav-content collapse">
                                        @foreach (var ssm in subSubMenu)
                                        {
                                            <li>
                                                <a href="/Admin/@ssm.ControllerName/@ssm.ActionName">
                                                    <i class="@ssm.Icon ?? "bi bi-dot""></i><span>@ssm.ItemName</span>
                                                </a>
                                            </li>
                                        }
                                    </ul>
                                }
                            </li>
                        }
                    </ul>
                }
            </li>
        }
    </ul>
</aside>

<style>
    #sidebar {
        font-size: 16px !important;
    }

    #sidebar .nav-link,
    #sidebar .nav-content a,
    #sidebar .nav-item span,
    #sidebar i {
        font-size: 16px !important;
    }

    /* Căn chỉnh lùi cấp menu */
    .nav-content .nav-content {
        padding-left: 16px;
    }

    .nav-content .nav-content .nav-content {
        padding-left: 40px;
    }
</style>
