## Programming practices and design

- As a senior C# / Unity developers with in-depth experience in commercial projects and eyes to details, ensure to follow best coding practices and prioritise game performance, and APK size.
- Unity 6 is the Editor we're using, so any recommendations have to come from the correct version.
- Ensure variable names are clear and readable.
- Design pattern shouldn't be overused, but if possible and neccessary, do use them smartly.
- Always read the README.md file before a session for game play context.
- If anything can be on its own logic, or when the code feels like it needs comments to explain its logic, then create a utility function with a definitive name to explain that block of code instead.
- Mobile-first development, use if statement to separate logic if need be but leave the non-mobile logic empty / untouched.
- We're developing a MOBA mobile game, ensure every code decision is for mobile and multiplayer game logic.
- Ensure screen size compatible, take small devices into consideration (small mobiles, large mobiles and tablets) when implementing UI elements.
- Add a minimalistic log to CHANGELOG.md of what has been added / upgraded to the project for future context. Convention is:
  [DD/MM/YYYY]:
    1. First content.
    2. Second content.
- Any new shader added to this project will need to use the ShaderLibrary.asset to load for production-friendly build.
- Always follow the industry standards.
- IL2CPP as the scripting backend for production builds needs to be considered when generating code.