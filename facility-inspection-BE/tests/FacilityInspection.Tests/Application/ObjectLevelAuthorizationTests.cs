using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Features.Incidents.Common;
using FacilityInspection.Application.Features.WorkOrders.Common;
using FacilityInspection.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FacilityInspection.Tests.Application;

/// <summary>
/// Verifies the resource-based (object-level) authorization gates that back the security
/// requirements the role attributes alone cannot express:
/// "a vendor cannot view another vendor's work orders" and "an initiator cannot view someone
/// else's incident". Admin/Inspector retain full read access.
/// </summary>
public class ObjectLevelAuthorizationTests
{
    private static Mock<ICurrentUserService> UserWith(string userId, params string[] roles)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.SetupGet(x => x.UserId).Returns(userId);
        mock.SetupGet(x => x.Roles).Returns(roles);
        return mock;
    }

    private static Mock<IIdentityService> IdentityWithVendor(Guid? vendorId)
    {
        var mock = new Mock<IIdentityService>();
        mock.Setup(x => x.GetUserVendorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendorId);
        return mock;
    }

    // ---------- WorkOrderAuthorization ----------

    [Fact]
    public async Task Vendor_cannot_view_another_vendors_work_order()
    {
        var myVendorId = Guid.NewGuid();
        var workOrder = new WorkOrder { VendorId = Guid.NewGuid(), Incident = new Incident() };
        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Vendor);
        var identity = IdentityWithVendor(myVendorId); // different from the work order's vendor

        var act = async () => await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, user.Object, identity.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Vendor_can_view_its_own_work_order()
    {
        var vendorId = Guid.NewGuid();
        var workOrder = new WorkOrder { VendorId = vendorId, Incident = new Incident() };
        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Vendor);
        var identity = IdentityWithVendor(vendorId);

        var act = async () => await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, user.Object, identity.Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Initiator_can_view_work_order_on_own_incident()
    {
        var userId = Guid.NewGuid();
        var workOrder = new WorkOrder
        {
            VendorId = Guid.NewGuid(),
            Incident = new Incident { ReportedByUserId = userId },
        };
        var user = UserWith(userId.ToString(), AppRoles.Initiator);

        var act = async () => await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, user.Object, IdentityWithVendor(null).Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Initiator_cannot_view_work_order_on_another_users_incident()
    {
        var workOrder = new WorkOrder
        {
            VendorId = Guid.NewGuid(),
            Incident = new Incident { ReportedByUserId = Guid.NewGuid() },
        };
        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Initiator);

        var act = async () => await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, user.Object, IdentityWithVendor(null).Object, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Theory]
    [InlineData(AppRoles.Admin)]
    [InlineData(AppRoles.Inspector)]
    public async Task Admin_and_inspector_can_view_any_work_order(string role)
    {
        var workOrder = new WorkOrder
        {
            VendorId = Guid.NewGuid(),
            Incident = new Incident { ReportedByUserId = Guid.NewGuid() },
        };
        var user = UserWith(Guid.NewGuid().ToString(), role);

        var act = async () => await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, user.Object, IdentityWithVendor(null).Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    // ---------- IncidentAuthorization ----------

    [Fact]
    public async Task Initiator_can_view_own_incident()
    {
        var userId = Guid.NewGuid();
        var incident = new Incident { ReportedByUserId = userId };
        var user = UserWith(userId.ToString(), AppRoles.Initiator);

        var act = async () => await IncidentAuthorization.EnsureCanViewAsync(
            incident, user.Object, IdentityWithVendor(null).Object,
            new Mock<IUnitOfWork>().Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Initiator_cannot_view_another_users_incident()
    {
        var incident = new Incident { ReportedByUserId = Guid.NewGuid() };
        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Initiator);

        var act = async () => await IncidentAuthorization.EnsureCanViewAsync(
            incident, user.Object, IdentityWithVendor(null).Object,
            new Mock<IUnitOfWork>().Object, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Vendor_can_view_incident_it_holds_a_work_order_for()
    {
        var vendorId = Guid.NewGuid();
        var incident = new Incident { ReportedByUserId = Guid.NewGuid() };

        var workOrders = new Mock<IWorkOrderRepository>();
        workOrders.Setup(x => x.GetByIncidentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new WorkOrder { VendorId = vendorId } });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.WorkOrders).Returns(workOrders.Object);

        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Vendor);

        var act = async () => await IncidentAuthorization.EnsureCanViewAsync(
            incident, user.Object, IdentityWithVendor(vendorId).Object,
            unitOfWork.Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Vendor_cannot_view_incident_it_has_no_work_order_for()
    {
        var incident = new Incident { ReportedByUserId = Guid.NewGuid() };

        var workOrders = new Mock<IWorkOrderRepository>();
        workOrders.Setup(x => x.GetByIncidentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkOrder>()); // vendor has no work order on this incident

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.WorkOrders).Returns(workOrders.Object);

        var user = UserWith(Guid.NewGuid().ToString(), AppRoles.Vendor);

        var act = async () => await IncidentAuthorization.EnsureCanViewAsync(
            incident, user.Object, IdentityWithVendor(Guid.NewGuid()).Object,
            unitOfWork.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
