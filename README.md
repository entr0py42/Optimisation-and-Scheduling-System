# Optimization & Scheduling System
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

A powerful web-based system built with **ASP.NET MVC** to solve complex **driver scheduling problems** using optimization techniques. Designed for **both driver & manager-level users**, it integrates optimization logic, data persistence, and intuitive results presentation in a clean, extensible architecture.

---

## Features

- **Driver Scheduling Optimization**  
  Automatically assign drivers to shifts using a Gurobi-powered optimization model. Ensures fairness, efficiency, and coverage.

- **Preference-Based Assignment**  
  Drivers submit their preferred routes and shifts. The optimization algorithm takes these preferences into account, balancing them with coverage requirements and performance scores.

- **Performance-Aware Scheduling**  
  Drivers with higher performance scores are favored when multiple candidates are available, ensuring quality in shift allocation.

- **Gurobi Optimization Engine**  
  Integrates with [Gurobi](https://www.gurobi.com/) to solve complex integer programming models efficiently and reliably.

- **PostgreSQL Database Support**  
  Stores driver data, preferences, route info, and credentials in a PostgreSQL database for robust, scalable persistence.

- **Persistent Result Storage**  
  Optimization results are saved and can be reloaded later. Users don't have to re-run the solver every time.

- **Scheduling Results View**  
  Results are shown in a user-friendly format with assignments and nested backup options for each shift.

- **Role-Based Access Control**  
  Built-in authorization ensures that only users with the `Manager` role can access and run optimizations.

- **Modular, Testable Architecture**  
  Decoupled service layer enables testing and future upgrades without major rewrites.

- **Backup Assignments**  
  Automatically includes secondary driver assignments to cover for no-shows or emergencies.



## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!