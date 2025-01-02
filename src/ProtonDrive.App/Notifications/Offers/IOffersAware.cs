namespace ProtonDrive.App.Notifications.Offers;

public interface IOffersAware
{
    void OnActiveOfferChanged(Offer? offer);
}
