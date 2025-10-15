# User Stories

## Format
Each story follows: **As a [persona], I want [goal], so that [benefit]**

## Personas
- **Beginner Bob**: New to LLMs, needs guidance
- **Developer Dana**: Experienced, wants quick technical info
- **Both**: Applies to both personas

---

## Epic 1: Hardware Assessment

### US-1.1: View Hardware Capabilities
**As Both**, I want to see my hardware specifications at a glance, so that I understand what I'm working with.

**Acceptance Criteria**:
- [ ] App auto-detects CPU, GPU, RAM, VRAM, Storage on startup
- [ ] Detection completes within 2 seconds
- [ ] Results displayed in clear, organized format
- [ ] Each component shows key specs (model, capacity, speed)
- [ ] If detection fails, show "Unknown" with explanation

**Priority**: P0 (Critical)

---

### US-1.2: Understand My Scores
**As Beginner Bob**, I want to see simple scores that tell me if my PC is good for LLMs, so that I don't need to understand technical details.

**Acceptance Criteria**:
- [ ] Three main scores displayed: Inference, Training, Fine-tuning
- [ ] Each score shows 0-1000 scale with color coding (red/yellow/green)
- [ ] Overall rating in plain English ("Excellent", "Good", "Fair", etc.)
- [ ] Tooltips explain what each score means
- [ ] Visual indicators (circular progress bars or gauges)

**Priority**: P0 (Critical)

---

### US-1.3: See Technical Details
**As Developer Dana**, I want access to raw hardware specifications and scoring breakdown, so that I can understand the methodology.

**Acceptance Criteria**:
- [ ] Advanced mode shows detailed specs (cache sizes, architecture, CUDA compute capability)
- [ ] Scoring breakdown showing how each component contributes
- [ ] Formula explanations available
- [ ] Raw JSON export option
- [ ] Comparison with benchmark data

**Priority**: P1 (High)

---

## Epic 2: Framework Compatibility

### US-2.1: Check Framework Support
**As Both**, I want to know which AI frameworks my system supports, so that I know what tools I can use.

**Acceptance Criteria**:
- [ ] Check for CUDA installation and version
- [ ] Check for ROCm (if AMD GPU)
- [ ] Check for DirectML (Windows)
- [ ] Check for oneAPI
- [ ] Show green checkmark for available, red X for missing
- [ ] Provide installation links for missing frameworks

**Priority**: P0 (Critical)

---

### US-2.2: WSL2 Detection (Windows)
**As Both**, I want to know my WSL2 status and configuration, so that I can use Linux-based tools on Windows.

**Acceptance Criteria**:
- [ ] Detect if WSL2 is installed
- [ ] Show WSL2 version
- [ ] Check if GPU passthrough is enabled
- [ ] Provide setup guide if not configured
- [ ] Link to official Microsoft WSL documentation

**Priority**: P2 (Medium)

---

## Epic 3: Model Recommendations

### US-3.1: See Compatible Models
**As Both**, I want to see which popular models will work on my hardware, so that I don't waste time downloading incompatible models.

**Acceptance Criteria**:
- [ ] Display list of popular models with compatibility status
- [ ] Each model shows: name, size, compatibility (yes/no/maybe)
- [ ] Filter by: use case (coding, chat, general), size, compatibility
- [ ] Sort by: recommended, popularity, name, size
- [ ] "Show only compatible" toggle
- [ ] At least 50 models in database

**Priority**: P0 (Critical)

---

### US-3.2: Understand Model Requirements
**As Beginner Bob**, I want to see simple explanations of what each model needs, so that I can make informed decisions.

**Acceptance Criteria**:
- [ ] For each model, show: VRAM needed, RAM needed, storage needed
- [ ] Color-coded status: green (you can run this), yellow (might work with optimization), red (not recommended)
- [ ] Plain language explanation: "Your GPU has enough VRAM" or "You need 8GB more VRAM"
- [ ] Estimated performance: "~25 tokens/sec" with explanation

**Priority**: P0 (Critical)

---

### US-3.3: Compare Quantization Options
**As Developer Dana**, I want to see performance trade-offs between quantization levels (4-bit, 8-bit, FP16, FP32), so that I can optimize for my use case.

**Acceptance Criteria**:
- [ ] Each model shows requirements for all quantization levels
- [ ] Performance estimates for each level
- [ ] Quality vs speed trade-off explanation
- [ ] Recommended quantization level highlighted
- [ ] Side-by-side comparison view

**Priority**: P1 (High)

---

### US-3.4: Get Personalized Recommendations
**As Both**, I want the app to recommend specific models for my hardware and use case, so that I have a clear starting point.

**Acceptance Criteria**:
- [ ] "Recommended for you" section on dashboard
- [ ] Top 3-5 model suggestions based on:
  - Hardware capabilities
  - Balanced requirements
  - Popularity
  - Beginner-friendly (in beginner mode)
- [ ] Each recommendation includes: why it's recommended, expected performance, setup difficulty

**Priority**: P0 (Critical)

---

## Epic 4: Training & Fine-tuning Assessment

### US-4.1: Know Training Capabilities
**As Developer Dana**, I want to know if I can train models locally, so that I can decide between local vs cloud training.

**Acceptance Criteria**:
- [ ] Separate "Training Score" prominently displayed
- [ ] For each model, show training requirements for:
  - Full fine-tuning
  - LoRA
  - QLoRA
- [ ] Estimated training time per epoch
- [ ] Clear indication if training is not recommended
- [ ] Comparison with cloud options (time & cost estimate)

**Priority**: P0 (Critical)

---

### US-4.2: Understand Training Limitations
**As Beginner Bob**, I want to understand why I can or cannot train certain models, so that I can set realistic expectations.

**Acceptance Criteria**:
- [ ] Plain language explanation of training requirements
- [ ] Bottleneck identification: "Your VRAM is the limiting factor"
- [ ] Visual showing what's needed vs what you have
- [ ] Educational content explaining full fine-tuning vs LoRA vs QLoRA
- [ ] "What would I need?" section showing requirements for training

**Priority**: P1 (High)

---

## Epic 5: Upgrade Advisor

### US-5.1: Identify Bottlenecks
**As Both**, I want to know what's limiting my system, so that I can prioritize upgrades.

**Acceptance Criteria**:
- [ ] Visual bottleneck analysis (chart/diagram)
- [ ] Ranked list of limitations (GPU VRAM, RAM, Storage, etc.)
- [ ] For each bottleneck, show:
  - Current value
  - Recommended value for target use case
  - Impact on capabilities (unlock X models, improve Y score)

**Priority**: P0 (Critical)

---

### US-5.2: Get Upgrade Recommendations
**As Both**, I want specific upgrade suggestions with cost estimates, so that I can plan my budget.

**Acceptance Criteria**:
- [ ] Ranked list of upgrade options by "bang for buck"
- [ ] For each upgrade:
  - Specific component recommendations (e.g., "RTX 4070 12GB")
  - Price range ($, $$, $$$)
  - Expected score improvement
  - Models/capabilities unlocked
- [ ] "If you upgrade X, you can run Y models"
- [ ] Multiple tiers (budget, mid-range, high-end)

**Priority**: P0 (Critical)

---

### US-5.3: Compare Before/After
**As Both**, I want to see a comparison of my current system vs upgraded system, so that I can visualize the improvement.

**Acceptance Criteria**:
- [ ] Side-by-side view: current vs proposed upgrade
- [ ] Score improvements clearly shown
- [ ] Model compatibility changes highlighted
- [ ] Performance improvements estimated
- [ ] Visual comparison (charts)

**Priority**: P1 (High)

---

### US-5.4: Software Upgrades
**As Both**, I want recommendations for software/driver updates, so that I can maximize my current hardware.

**Acceptance Criteria**:
- [ ] Check driver versions (GPU, chipset)
- [ ] Recommend CUDA toolkit installation if missing
- [ ] Suggest WSL2 setup (Windows)
- [ ] Framework installation guides
- [ ] Step-by-step instructions with links

**Priority**: P1 (High)

---

## Epic 6: Educational Content

### US-6.1: Learn Why Specs Matter
**As Beginner Bob**, I want to understand why each hardware component is important, so that I can make informed decisions.

**Acceptance Criteria**:
- [ ] Educational section accessible from dashboard
- [ ] Topics covered:
  - What is VRAM and why it matters
  - GPU vs CPU for LLMs
  - What are tokens/sec
  - Quantization explained
  - Training vs inference differences
- [ ] Simple language with visuals
- [ ] Interactive examples
- [ ] "Learn more" links throughout app

**Priority**: P1 (High)

---

### US-6.2: Interactive Tutorials
**As Beginner Bob**, I want guided tutorials that walk me through the app, so that I don't feel overwhelmed.

**Acceptance Criteria**:
- [ ] First-time user tutorial (skippable)
- [ ] Step-by-step walkthrough of main features
- [ ] Contextual help tooltips
- [ ] "What's this?" buttons next to technical terms
- [ ] Tutorial can be re-launched from settings

**Priority**: P2 (Medium)

---

### US-6.3: Glossary
**As Both**, I want a searchable glossary of AI/hardware terms, so that I can look up unfamiliar concepts.

**Acceptance Criteria**:
- [ ] Comprehensive glossary (50+ terms)
- [ ] Search functionality
- [ ] Categories: Hardware, AI/ML, Model Types, Frameworks
- [ ] Cross-references between related terms
- [ ] Accessible from any view

**Priority**: P2 (Medium)

---

## Epic 7: Benchmarking (Optional)

### US-7.1: Run Quick Benchmark
**As Both**, I want to run a quick performance test, so that I can verify the estimates with real data.

**Acceptance Criteria**:
- [ ] "Run Benchmark" button on dashboard
- [ ] Downloads small test model (~100MB) on first run
- [ ] Quick test completes in ~30 seconds
- [ ] Shows real-time progress
- [ ] Results: tokens/sec, time to first token, memory usage
- [ ] Compares real results vs estimates

**Priority**: P2 (Medium)

---

### US-7.2: Comprehensive Benchmark
**As Developer Dana**, I want to run thorough performance tests with multiple models/prompts, so that I have detailed performance data.

**Acceptance Criteria**:
- [ ] "Comprehensive Test" option (~5 minutes)
- [ ] Tests multiple prompt lengths
- [ ] Tests different batch sizes
- [ ] Sustained performance measurement
- [ ] Detailed results breakdown
- [ ] Export results (JSON/CSV)
- [ ] Share results to community database (opt-in)

**Priority**: P3 (Low)

---

## Epic 8: Settings & Customization

### US-8.1: Switch Modes
**As Both**, I want to easily switch between Beginner and Advanced modes, so that I can adjust the complexity to my comfort level.

**Acceptance Criteria**:
- [ ] Toggle in settings: Beginner / Advanced
- [ ] Beginner mode:
  - Simplified language
  - More explanations
  - Fewer options shown
  - Guided workflows
- [ ] Advanced mode:
  - Technical terminology
  - All options visible
  - Raw data access
  - Less hand-holding
- [ ] Mode persists across sessions

**Priority**: P0 (Critical)

---

### US-8.2: Theme Selection
**As Both**, I want to choose between light and dark themes, so that the app is comfortable to use in different lighting conditions.

**Acceptance Criteria**:
- [ ] Light and dark themes available
- [ ] System theme detection (default)
- [ ] Manual override option
- [ ] Smooth transition between themes
- [ ] All views support both themes

**Priority**: P1 (High)

---

### US-8.3: Model Database Updates
**As Both**, I want the app to automatically check for model database updates, so that I always have the latest model information.

**Acceptance Criteria**:
- [ ] Check for updates on startup (with timeout)
- [ ] Download new database if available
- [ ] Show update notification
- [ ] Manual "Check for updates" button
- [ ] Works offline with cached database
- [ ] Shows last update timestamp

**Priority**: P0 (Critical)

---

### US-8.4: Data Sharing Opt-in
**As Both**, I want to optionally share my benchmark results with the community, so that I can contribute to better estimates for everyone.

**Acceptance Criteria**:
- [ ] Opt-in during first run (default: off)
- [ ] Clear explanation of what's shared (anonymous hardware profile + benchmarks)
- [ ] Can enable/disable anytime in settings
- [ ] Shows what data would be shared
- [ ] "Share this result" button after each benchmark
- [ ] Never shares personally identifiable information

**Priority**: P2 (Medium)

---

### US-8.5: Export/Import Settings
**As Developer Dana**, I want to export and import my settings/profiles, so that I can test different configurations or share setups.

**Acceptance Criteria**:
- [ ] Export settings to JSON
- [ ] Import settings from JSON
- [ ] Export hardware profile (for sharing/reporting)
- [ ] Export benchmark results
- [ ] Export full report (PDF/HTML)

**Priority**: P3 (Low)

---

## Epic 9: User Experience

### US-9.1: Fast Startup
**As Both**, I want the app to start quickly, so that I don't wait around.

**Acceptance Criteria**:
- [ ] Cold start under 3 seconds
- [ ] Warm start under 1 second
- [ ] Show splash screen with progress indicator
- [ ] Hardware detection runs in background
- [ ] UI responsive immediately

**Priority**: P0 (Critical)

---

### US-9.2: Smooth Navigation
**As Both**, I want to easily navigate between different sections, so that I can find what I need quickly.

**Acceptance Criteria**:
- [ ] Clear navigation menu (sidebar or top bar)
- [ ] Sections: Dashboard, Hardware, Models, Upgrade Advisor, Benchmark, Education, Settings
- [ ] Back button where appropriate
- [ ] Breadcrumbs for deep navigation
- [ ] Keyboard shortcuts (Alt+1, Alt+2, etc.)

**Priority**: P0 (Critical)

---

### US-9.3: Responsive UI
**As Both**, I want the app to feel smooth and responsive, so that it's pleasant to use.

**Acceptance Criteria**:
- [ ] 60 FPS animations
- [ ] No UI freezing during operations
- [ ] Loading indicators for long operations
- [ ] Async operations don't block UI
- [ ] Instant feedback on clicks/interactions

**Priority**: P1 (High)

---

### US-9.4: Helpful Error Messages
**As Both**, I want clear error messages when something goes wrong, so that I know what to do.

**Acceptance Criteria**:
- [ ] No technical error codes in beginner mode
- [ ] Plain language explanation of problem
- [ ] Suggested solutions
- [ ] "Report Issue" button with diagnostic info
- [ ] Error details available in advanced mode

**Priority**: P1 (High)

---

## Epic 10: Information Architecture

### US-10.1: Search Functionality
**As Both**, I want to search for models, terms, or features, so that I can quickly find what I need.

**Acceptance Criteria**:
- [ ] Global search box (Ctrl+F)
- [ ] Search in: models, glossary, help content
- [ ] Real-time search results
- [ ] Highlight matches
- [ ] Recent searches saved

**Priority**: P2 (Medium)

---

### US-10.2: Contextual Help
**As Both**, I want help available where I need it, so that I don't have to search for answers.

**Acceptance Criteria**:
- [ ] "?" icon next to complex features
- [ ] Tooltip help on hover
- [ ] "Learn more" links to relevant education section
- [ ] Context-sensitive help panel
- [ ] F1 opens help for current view

**Priority**: P1 (High)

---

## Epic 11: Comparison Features

### US-11.1: Compare My System with Others
**As Developer Dana**, I want to see how my system compares to community benchmarks, so that I know if I'm getting expected performance.

**Acceptance Criteria**:
- [ ] "Compare with community" view
- [ ] Show percentile ranking (top 10%, top 50%, etc.)
- [ ] Filter by similar configurations
- [ ] Chart showing distribution of scores
- [ ] See configurations better/worse than yours

**Priority**: P3 (Low)

---

### US-11.2: Track Changes Over Time
**As Both**, I want to track how my scores change over time (after upgrades/driver updates), so that I can see improvements.

**Acceptance Criteria**:
- [ ] Store historical data locally
- [ ] Chart showing score trends
- [ ] Annotate upgrades/changes
- [ ] Export historical data
- [ ] Compare any two snapshots

**Priority**: P3 (Low)

---

## Epic 12: Advanced Features

### US-12.1: Multi-GPU Support (Future)
**As Developer Dana**, I want to assess multi-GPU setups, so that I can plan distributed training or inference.

**Acceptance Criteria**:
- [ ] Detect all GPUs in system
- [ ] Aggregate scores
- [ ] Show per-GPU details
- [ ] Recommend models that support multi-GPU
- [ ] Estimate scaling efficiency

**Priority**: P4 (Future)

---

### US-12.2: Custom Model Entry (Future)
**As Developer Dana**, I want to input custom model requirements, so that I can assess models not in the database.

**Acceptance Criteria**:
- [ ] Manual model entry form
- [ ] Fields: name, parameters, quantization, requirements
- [ ] Run compatibility check
- [ ] Save custom models
- [ ] Share custom models with community

**Priority**: P4 (Future)

---

## Acceptance Criteria Summary

### Definition of Done
For each user story to be considered complete:
- [ ] Feature implemented and functional
- [ ] Unit tests written and passing
- [ ] Integration tests passing
- [ ] UI responsive and performant
- [ ] Works in both beginner and advanced modes (if applicable)
- [ ] Error handling in place
- [ ] Documentation updated
- [ ] Manually tested on Windows 10 and 11
- [ ] No regressions in existing features

### Story Point Estimates
- P0 (Critical): Must have for MVP - 89 points total
- P1 (High): Should have for MVP - 34 points total
- P2 (Medium): Nice to have for MVP - 21 points total
- P3 (Low): Post-MVP - 13 points total
- P4 (Future): Future versions - 8 points total

**Total MVP scope**: 144 story points
**Estimated dev time**: 6-8 weeks for 1 developer
