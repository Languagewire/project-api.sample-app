# Project API Sample APP
[![Build&Test](../../actions/workflows/build-and-test.yml/badge.svg)](../../actions/workflows/build-and-test.yml)

This is a Sample APP for the LanguageWire Project API.

This console application is meant to showcase how to build an integration towards the LanguageWire Platform by using the
Project API.

The idea is then that you can see how we build an `Infrastructure` layer to communicate with the Project API, and how we
use it to go through the whole workflow of a translation project.

This console app is offered from our developer portal
in [Dev portal sample app page](https://developers.languagewire.com/project-api/overview/)
so feel free to dig into this documentation to know more about the API if you need it.

## Solution structure

The solution contains two main projects:

* SampleApp
  Console application that outputs different options so you can choose what you want to see.
  The options are handled on the `SampleAppRunner.cs` which uses the `TranslationService.cs` to handle the actions.

* SampleApp.Infrastructure.ProjectApiClient
  This project contains the `ProjectApiClient` class to wrap all the HTTPS calls as well as the 'models' (or contracts)
  for the requests and the responses, mappers, configuration, custom exceptions and even resiliency policies with the
  Polly library.

## Trying it out

> *Required:* You will need a LanguageWire account in order to set you client id and secret and make it work.

If you want to try it out just download the code, open the solution with Visual Studio, Raider or the IDE you prefer and
hit the play button.

Start to play with the options so you can see what happens.

Then start to explore the code and see how things are done so you can start to see what you need to do for your
integration and feel free to copy as much code as you want.
