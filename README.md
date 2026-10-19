# Smart-X IoT Mesh Ecosystem - Part 2

A high-performance, hybrid IoT platform built with .NET 10, ASP.NET Core Minimal API, and Blazor WebAssembly.

## Features
- **Part 1:** Sensor registration, telemetry ingestion, file uploads, and progressive disclosure dashboards.
- **Part 2:** Real-Time Command Stream, live queue processing, manual device overrides, and a predictive recommendation engine.

## Advanced Data Structures Implemented
- **Queues:** `ConcurrentQueue` handles standard First-In-First-Out telemetry.
- **Priority Queues:** Critical anomalies (e.g., >100) bypass the standard queue.
- **Stacks:** `ConcurrentStack` powers the "Undo Last Command" feature.
- **Dictionaries:** `Dictionary<string, SensorRegistration>` provides O(1) instant device lookup.
- **Sorted Dictionaries:** Chronological history for the live timeline.
- **Hash Sets:** Prevents duplicate processing of disconnected node alerts.

##  How to Run
1. Open `SmartX.slnx` in Visual Studio 2022.
2. Set both `SmartX.Api` and `SmartX.Client` as startup projects.
3. Press F5.
