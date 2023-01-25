# Ads Bridge for Unity

The intention of this document is to explain how a generic Bridge pattern allow us to migrate between different Mediations. This component was first developed when migrating Frozen Adventures from Unity Mediation to Ads SDK + IronSource Bridge.

To know more about Ads SDK, please refer to GS documentation.

Please, in order to maintain the component updated for the team, if any change arises or a new Bridge is created, please submit a Pull Request to update the component.

# Implementation Goal

The main goal of the architecture decided for this feature is to create an intermediate layer between the Game and the Ads Mediator utilized by the game, so it can be easily switchable without needing to make a lot of changes.

The reason behind this approach, is that normally all games have a different implementation to retrieve ads, normally wrapped on a class or object called AdsService.

Even if AdsService is normally handled as an abstraction, the object tends to have other dependencies like configurations, analytics and more, so changing the Ads Provider tends to lend to a refractor that, not only increments the cost of the feature, but also can break the already working components like the analytics and configurations.

By creating a bridge between the Game's Ads Service and the Ads Provider by itself, we allow the game to only change the Mediator without needing to modify the game's Ads Service. So, that way, the Ads Service communicates with all the mediators in the same way, avoiding the necessity of modifying the other components too.


That way, we see that the only thing that changes is the Bridge while AdService is still untouched, and we can create a Bridge between different Mediators.

Note: Take into account that only one mediator can exist in a project due to conflicts, so the image above is illustrative to show how the mediator can be changed.


# How does it Work

This implementation is based over a single interface called IAdsMediator. This interface has only the required methods for a Game to interact with an Ads Provider, being these methods: Initialize, IsAdLoaded and ShowAd (receiving the type and placement of the ads by parameters). Also parameters to check for Initialization Status are added to the interface.

To implement a Mediator, the Engineer should only create a new class inheriting from IAdsMediator interface and implement the members by accessing the real Mediator's SDK calls.

So, to work with the provider, the Game AdsService should only define a IAdsMediator variable and assign the bridge that it wants to use, and then just initialize the component by calling Initialize and call Ads by just calling IsAdLoaded and ShowAd methods.

If at any moment the game needs to switch the Mediator again, the process is the same. A new class inheriting from IAdsMediator should be created, that class should work with the new Mediator, and the AdsService should only change the variable described above to switch the component, but being that it will be working through an interface, nothing else should be done to switch from one mediator to another.

# Configuration Asset

The Initialize method of the IAdsMediator interface requires an object of type AdsMediationServiceConfig. This class is a Scriptable Object in which we can define the Game ID to connect with the mediator, the Rewarded Ad IDs and the Interstitial Ad IDs. Both for Android and iOS.

This Scriptable Object can be created by right clicking on the Project window -> Create -> Ads Implementation -> Ads Mediator Config (inside of Unity).

Even if all Ad Providers have a different kind of ID for placements and Game IDs, the element is normally a string with different format. (i.e. IronSource have specific placement names while AdMob have hash-kind of strings)

So, this object should contain the Game IDs and all the required Ad Placements that should be used by the Mediator.

Then, we need to load this object when creating the IAdsMediator and send this object through the Initialize method in order to configure the Mediator.

Of course, each Bridge should implement their logic to parse this configuration file into the Mediator requirements in their Initialize method implementation.



# Listening Ads States

A normal situation when working with Ads, is the necessity of listening to what happens with them when we request loading or showing them.
For example, normal callbacks for Ads are:

- If it is loaded
- If it failed to load
- If it is being showed
- If it failed to show
- If it was clicked
- If it was closed
- If the user should receive a reward.



Normally, games tend to listen to those callbacks to either notify the player if something happened  or give a reward to the player.

Even if all Mediators normally support these kind of callbacks, the implementation of them are different, so if we were going to work explicitly over the mediator in our Ads Service, we would need to change all that calls, being very error prompt.

By working with our own bridges, the bridge itself is in charge of handling these events and notifying everyone that wants to know what happened with the Ads, but the listeners don't need to adapt all their calls if the Mediator was changed.

To to this, the IAdsMediator interface commented above supports a Subscription pattern.

By doing that, we can Subscribe any kind of object that implements the IAdsListener interface, and each time an event is dispatched from the mediator, all the listeners will receive that information.

So, to listen to Ad events, what we can do is implement the IAdsListener interface in our AdsService, define what should happen on each call, and then subscribe the Ads Service to the Mediator. That way, the AdsService will know everything that happens with Ads and can notify the player correctly.



# Dummy Mediator

A simple Dummy Mediator was implemented in order to test the game functionality without needing to have a specific mediator and for Unit Testing.

This mediator is configurable so it can retrieve what the user wants to test the implementation of their service before passing it to the real Mediator. To use this, just define a DummyMediator object into your IAdsMediator reference.



# Implementing the Bridge

The first implementation of this pattern can require changes due to adapting the game's Ad Service to work with this pattern, and that normally refers to extracting all the already implemented mediator calls into a different object.

So, in order to implement this Pattern, the following approach is suggested:

Implement the Ads Bridge Unity Package to have the required interfaces and objects to start the migration.

Create a new IAdsMediator inheritance class and make all the SDK calls of the already implemented Mediator into this class. (If it not exists yet)

Remove all the legacy calls to the Mediator that are implemented and substitute them with the new IAdsMediator variable and calls. Also implement the IAdsListener interface to your service if needed.

Validate that the game is running correctly with the same mediator but interacting with the new bridge.

Once everything is working correctly, the migration can be done.

Remove the old Mediator SDK and packages from the project.

Implement the new Mediator SDK that you need to implement.

Create a IAdsMediator inheritance class of the new mediator and adapt the calls to work with the new Mediator.

Replace the variable in your Ads Service with the new mediator class.

Check that everything is working with the new mediator.

If anything is not working correctly after doing these steps, that means that your new Mediator inheritance can have something missing being that it was working with the previous one. So all the major changes now should be done in the bridge inheritance instead of the main game Ads Service.

Existing Bridges

Note: These bridges were created to migrate existing games to this implementation. Each Bridge is defined on code inside a specific Symbol because they require third party packages to work, and if they are not implemented when implementing the script, the game won't compile.

Each Bridge exists inside the “Implementations” folder of the Repository. Be sure to implement only the ones that your project needs.

Unity Mediation Bridge (Requires Unity Mediation package)

Ads SDK + Iron Source Bridge (Requires JC Ads SDK and JC IronSource Bridge packages)
