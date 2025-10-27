# Roadmap

This roadmap outlines the planned development for LLM Capability Checker. All timelines are estimates and subject to change based on community feedback and contributions.

## Near Term (v1.1 - v1.2)

**Focus: Polish and usability improvements**

- [ ] **HTML/Markdown Report Export** - Generate shareable reports with hardware specs and model recommendations
- [ ] **Model Database Editor** - In-app tool to add/edit models without touching JSON
- [ ] **CLI Mode** - Run hardware check from command line and output results to stdout
- [ ] **Improved Model Search** - Filter by model family, parameter count, and quantization
- [ ] **Custom Model Entry** - Manually enter model specs for testing compatibility
- [ ] **Performance Profiling** - Identify and fix slow hardware detection on specific platforms

## Mid Term (v1.3 - v2.0)

**Focus: Advanced features and CI/CD integration**

- [ ] **Multi-GPU Support** - Detect and score systems with multiple GPUs
- [ ] **Historical Tracking** - Track hardware changes and score trends over time
- [ ] **CI/CD Mode** - Fail builds if system requirements drop below threshold
- [ ] **Benchmark Suite** - Optional real-world performance tests (with user consent)
- [ ] **Cloud Provider Comparisons** - Compare your hardware vs AWS/Azure/GCP GPU instances
- [ ] **Model Download Integration** - One-click download compatible models via Ollama/HuggingFace

## Stretch Goals (v2.x+)

**Focus: Community and ecosystem**

- [ ] **Community Database** - Opt-in sharing of real-world performance data
- [ ] **Hardware Marketplace** - Show used GPU prices and ROI for upgrades
- [ ] **Organization Policy Packs** - Pre-configured requirement sets for enterprises
  - Legal/compliance minimum hardware specs
  - Security baseline configurations
  - Performance SLA requirements
- [ ] **Plugin System** - Allow community extensions for custom hardware detection
- [ ] **Web API** - Host as a service for automated hardware validation
- [ ] **Mobile App** - Quick hardware check on phones/tablets

## Community Requests

Have a feature you'd like to see? [Open an issue](https://github.com/random-llama/llm-capability-checker/issues/new) with the `enhancement` label.

Most requested features get prioritized!

---

**Current Version**: v1.0.4
**Last Updated**: January 2025
