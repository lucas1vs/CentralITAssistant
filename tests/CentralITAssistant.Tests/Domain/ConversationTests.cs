
using CentralITAssistant.Domain.Entities;

namespace CentralITAssistant.Tests.Unit.Domain;

public class ConversationTests
{
    [Fact]
    public void Constructor_WithValidContact_ShouldCreateConversation()
    {
        var contact = new WhatsAppContact("5561999999999");

        var conversation = new Conversation(contact);

        Assert.NotEqual(Guid.Empty, conversation.Id);
        Assert.Equal(contact.Id, conversation.WhatsAppContactId);
        Assert.Equal(contact, conversation.WhatsAppContact);
        Assert.Null(conversation.EmployeeId);
        Assert.Null(conversation.Employee);
        Assert.True(conversation.IsActive);
        Assert.Null(conversation.EndedAt);
    }

    [Fact]
    public void Constructor_WithNullContact_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Conversation(null!));
    }

    [Fact]
    public void AssociateEmployee_WithValidEmployee_ShouldEstablishRelationship()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        var conversation = new Conversation(contact);

        conversation.AssociateEmployee(employee);

        Assert.Equal(employee.Id, conversation.EmployeeId);
        Assert.Equal(employee, conversation.Employee);
    }

    [Fact]
    public void AssociateEmployee_WithDifferentEmployee_ShouldThrowInvalidOperationException()
    {
        var firstEmployee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var secondEmployee = new Employee(
            "João",
            "joao@centralit.com.br");

        var contact = new WhatsAppContact("5561999999999");

        firstEmployee.AssociateWhatsAppContact(contact);

        var conversation = new Conversation(contact);

        Assert.Throws<InvalidOperationException>(() =>
            conversation.AssociateEmployee(secondEmployee));
    }

    [Fact]
    public void AssociateEmployee_WhenAlreadyAssociatedWithAnotherEmployee_ShouldThrowInvalidOperationException()
    {
        var employee = new Employee(
            "Lucas",
            "lucas@centralit.com.br");

        var contact = new WhatsAppContact("5561999999999");

        employee.AssociateWhatsAppContact(contact);

        var conversation = new Conversation(contact);

        conversation.AssociateEmployee(employee);

        var anotherEmployee = new Employee(
            "João",
            "joao@centralit.com.br");

        Assert.Throws<InvalidOperationException>(() =>
            conversation.AssociateEmployee(anotherEmployee));
    }

    [Fact]
    public void AssociateEmployee_WithNullEmployee_ShouldThrowArgumentNullException()
    {
        var contact = new WhatsAppContact("5561999999999");

        var conversation = new Conversation(contact);

        Assert.Throws<ArgumentNullException>(() =>
            conversation.AssociateEmployee(null!));
    }

    [Fact]
    public void Close_ShouldSetIsActiveToFalseAndSetEndedAt()
    {
        var contact = new WhatsAppContact("5561999999999");

        var conversation = new Conversation(contact);

        conversation.Close();

        Assert.False(conversation.IsActive);
        Assert.NotNull(conversation.EndedAt);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        var contact = new WhatsAppContact("5561999999999");

        var conversation = new Conversation(contact);

        conversation.Close();

        Assert.Throws<InvalidOperationException>(() =>
            conversation.Close());
    }
}
