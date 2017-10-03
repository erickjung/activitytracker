# ActivityTracker

## What is ActivityTracker?

ActivityTracker is an open-source and cross-platform .NET library used to track every dekstop OS process in a moment at time. It was initialy created to play with .NET Core and .NET Standard 2 and with a very straightforwad API, you can easyly integrate at your application.

## What you can do?

### ActivityTrackerCLI (Sample App)

With this tool you can automatically track applications you use during your desktop usage.

You can:
- Track your desktop process with a specific interval
- Store data locally with a simple JSON file
- Convert the JSON to a HTML report with charts

How to use:
1. Track desktop process every 10 seconds:
   ```
   $ dotnet run track out.json 10000
   ```

2. Create report with HTML and charts:
   ```
   $ dotnet run convert out.json 10000 out.html
   ```

Output:
![Report Example](/docs/report_example.png)


## Plans

We are at a very early stage of this lib, but if you want to help, please report issues. :)


## Roadmap

- OSX Support - DONE
- Windows Support - In Progress
- Linux Support - Planned

## License

MIT License - see the LICENSE file in the source distribution

