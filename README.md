# UnityTogether

**UnityTogether** is a hobby project, designed to enable real-time collaboration inside the Unity Editor. 
By leveraging a custom C# networking server, UnityTogether allows multiple users to work together seamlessly within the same Unity scene, improving productivity and fostering teamwork for developers, artists, and designers.

---

## Features

- **Real-Time Collaboration:**  
  Multiple users can connect to the same Unity project and see each other's changes live.

- **Custom Networking Server:**  
  The project includes a robust C# server built with .NET 6.0, specifically tailored for Unity Editor collaboration.

- **Unity Integration:**  
  Scripts, scenes, and assets are organized for easy integration and extension within any Unity project.

---

## Getting Started

### Prerequisites

- **Unity Editor** (recommended version: 2021.3 LTS or later)
- **.NET 6.0 SDK** (for building and running the server)

### 1. Clone the Repository

```bash
git clone https://github.com/Saphirah/UnityTogether.git
```

### 2. Set Up the Server

- Navigate to `UnityTogetherServer2.0 Source/UnityTogetherServer/`
- Build and run the server:
  ```bash
  dotnet build
  dotnet run
  ```
- The server will start and listen for incoming connections from Unity Editor clients.

### 3. Open the Project in Unity

- Launch Unity Hub and open the cloned project folder.
- Open your desired scene or create a new one.
- Use the provided UnityTogether MonoBehaviour to connect to the running server.

### 4. Collaborate!

- Each user should connect their Unity Editor to the same server instance.
- Changes made in one editor will be synchronized in real-time with all connected users.
- Assets will be automatically synchronized over the network.

The plugin simply synchronizes the changes happening in the editor. It does not enforce that every user has the same scene, assets, or same VSC commit. 
Please make sure, you start the session on a shared VSC commit.

---

## Contributing

Contributions are welcome!
This project is not in active development, but I will maintain and review pull requests.

---

## License

This project is licensed under the MIT License.  
