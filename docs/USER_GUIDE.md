# LLM Capability Checker - User Guide

Welcome to the LLM Capability Checker User Guide! This comprehensive guide will help you understand and use all features of the application.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Dashboard Overview](#dashboard-overview)
3. [Hardware Detection](#hardware-detection)
4. [Understanding Scores](#understanding-scores)
5. [Model Recommendations](#model-recommendations)
6. [Upgrade Advisor](#upgrade-advisor)
7. [Component Details](#component-details)
8. [Benchmark Testing](#benchmark-testing)
9. [Export Reports](#export-reports)
10. [Settings & Preferences](#settings--preferences)
11. [Troubleshooting](#troubleshooting)
12. [FAQ](#faq)

---

## Getting Started

### First Launch

When you first launch LLM Capability Checker:

1. **Automatic Detection**: The app automatically detects your hardware within 2-3 seconds
2. **Score Calculation**: Your system receives scores for running LLM models
3. **Model Recommendations**: Compatible models are displayed based on your hardware
4. **Upgrade Suggestions**: Areas for improvement are highlighted

### System Requirements

- **Windows 10/11** (64-bit), **Linux** (x64), or **macOS** (x64/ARM64)
- **.NET 8.0 Runtime** or later
- **100MB RAM** available
- **50MB Storage** space
- **1024x768** minimum display resolution

### Navigation

- **Dashboard**: Main screen showing overall system analysis
- **Component Details**: Click any component score to view detailed information
- **Settings**: Access via Settings button (top-right, gear icon)
- **About**: Application information (top-right, info icon)
- **Help**: Press **F1** or click Help menu for in-app assistance

---

## Dashboard Overview

The Dashboard is your main hub for system analysis. Here's what each section shows:

### Overall Score Card (Blue Section at Top)

**Large Score (0-100)**
- Your system's overall capability for running LLM models
- Higher is better: 80+ = Excellent, 60-79 = Good, 40-59 = Fair, 20-39 = Limited, <20 = Poor

**System Tier Badge**
- **Entry-Level**: Suitable for small models (1B-7B parameters)
- **Mid-Range**: Can handle medium models (7B-13B parameters)
- **High-End**: Capable of large models (13B-34B parameters)
- **Enthusiast**: Top-tier for 70B+ parameter models

**Recommended Model Size**
- The optimal size of models your system can run efficiently
- Example: "7B-13B models" means 7 to 13 billion parameter models

**Primary Bottleneck**
- The component limiting your LLM performance most
- Focus upgrade efforts here first

**Action Buttons**
- **Refresh**: Re-scan hardware (use after hardware changes)
- **Export Report**: Save detailed analysis to JSON file
- **Run Benchmark**: Test actual system performance (30-60 seconds)

### Hardware Information Cards (Gray Cards)

Four cards display your key hardware:

- **CPU**: Processor model and specifications
- **GPU**: Graphics card model and VRAM (if present)
- **RAM**: Total system memory
- **Storage**: Available space and drive type

Click any card to navigate to detailed component information.

### Component Scores (Progress Bars)

Five interactive score cards with progress bars:

1. **CPU Score** (0-100)
   - Based on: Core count, clock speed, generation
   - Impact: Model loading speed, CPU inference

2. **Memory Score** (0-100)
   - Based on: Total RAM capacity
   - Impact: Model size you can load

3. **GPU Score** (0-100)
   - Based on: VRAM, compute capability, architecture
   - Impact: Inference speed, model size on GPU

4. **Storage Score** (0-100)
   - Based on: Available space, drive type (HDD/SSD/NVMe)
   - Impact: Model loading time, storage of multiple models

5. **Framework Score** (0-100)
   - Based on: Detected ML frameworks (CUDA, DirectML, ROCm, Metal)
   - Impact: GPU acceleration availability

**Color Coding**:
- **Green** (80-100): Excellent, no upgrade needed
- **Orange** (60-79): Good, minor improvements possible
- **Red** (0-59): Needs attention, upgrade recommended

Click any score card to see detailed breakdown and specific recommendations.

### Upgrade Suggestions Grid

Six cards showing component-by-component upgrade advice:

- **CPU, GPU, RAM, Storage, Frameworks**: Individual upgrade suggestions appear when score < 80
- **System Summary**: Overall advice prioritizing most impactful upgrades

Each upgrade card shows:
- Current score and color-coded progress bar
- Current hardware specification
- Specific upgrade recommendation (when applicable)
- Estimated benefit

### Recommended Models

Green cards showing top 3 models compatible with your system:

- **Model Name**: e.g., "Llama 3.2 3B"
- **Description**: Brief overview of model capabilities
- **Parameter Size Badge**: Model size (e.g., "3B", "8B")
- **Performance Badge**: Expected tokens/second
- **Compatibility Score**: How well it matches your hardware (0-100%)

---

## Hardware Detection

### What Gets Detected

**CPU Information**:
- Model name and manufacturer
- Core count (physical and logical)
- Base and boost clock speeds
- Architecture generation

**GPU Information**:
- Model name and vendor (NVIDIA, AMD, Intel, Apple)
- VRAM (dedicated video memory)
- Compute capability
- Driver version

**Memory (RAM)**:
- Total system RAM
- Available RAM
- Memory type (DDR4, DDR5)
- Memory speed (MHz)

**Storage**:
- Primary drive capacity
- Available free space
- Drive type (HDD, SATA SSD, NVMe SSD)
- Read/write speeds (estimated)

**ML Frameworks**:
- CUDA (NVIDIA GPU acceleration)
- ROCm (AMD GPU acceleration)
- DirectML (Windows GPU acceleration)
- Metal (Apple GPU acceleration)
- OpenVINO (Intel optimization)

### Platform-Specific Detection

**Windows**:
- Uses Windows Management Instrumentation (WMI)
- Detects CUDA via `nvidia-smi` if available
- Checks for DirectML support

**Linux**:
- Uses command-line tools: `lscpu`, `lspci`, `lsblk`, `df`
- Detects CUDA via `nvidia-smi`
- Detects ROCm via `rocm-smi`

**macOS**:
- Uses `sysctl`, `system_profiler`, `diskutil`
- Automatically detects Metal support
- Identifies Apple Silicon vs Intel

### Manual Refresh

If you make hardware changes (install RAM, add GPU, etc.):

1. Click **Refresh** button on Dashboard
2. Wait 2-3 seconds for re-detection
3. Scores and recommendations update automatically

---

## Understanding Scores

### Overall Score (0-100)

Calculated as weighted average:
- **CPU Score**: 20% weight
- **Memory Score**: 25% weight
- **GPU Score**: 30% weight
- **Storage Score**: 15% weight
- **Framework Score**: 10% weight

**Score Ranges**:
- **80-100**: Excellent - Can run most models smoothly
- **60-79**: Good - Suitable for common use cases
- **40-59**: Fair - Limited to smaller models
- **20-39**: Limited - Basic models only
- **0-19**: Poor - Significant upgrades needed

### Component Score Breakdown

**CPU Score Factors**:
- **Cores**: 8+ cores = higher score
- **Clock Speed**: 3.5+ GHz boost = higher score
- **Generation**: Newer generations (Ryzen 5000+, Intel 12th gen+) = higher score
- **Architecture**: x86_64 required, ARM64 supported on macOS

**Memory Score**:
- **8GB**: Score ~30 (Minimal, 1B-3B models only)
- **16GB**: Score ~50 (Entry-level, 7B models with quantization)
- **32GB**: Score ~75 (Mid-range, 7B-13B models)
- **64GB**: Score ~90 (High-end, 13B-34B models)
- **128GB+**: Score ~100 (Enthusiast, 70B+ models)

**GPU Score Factors**:
- **VRAM**: Primary factor
  - 4GB = ~40 (Minimal)
  - 8GB = ~60 (Entry-level)
  - 12GB = ~75 (Mid-range)
  - 16GB = ~85 (High-end)
  - 24GB+ = ~100 (Enthusiast)
- **Compute Capability**: CUDA 7.0+ or equivalent
- **Architecture**: Newer = better (Ampere/Ada > Turing > Pascal)

**Storage Score**:
- **Type**: NVMe SSD (100) > SATA SSD (70) > HDD (30)
- **Available Space**:
  - <50GB = -20 penalty
  - 50-100GB = base score
  - 100-500GB = +10 bonus
  - 500GB+ = +20 bonus

**Framework Score**:
- **No Frameworks**: 30-40 (CPU-only inference)
- **DirectML Only**: 50-60 (Basic GPU acceleration)
- **CUDA/ROCm/Metal**: 80-100 (Full GPU acceleration)
- **Multiple Frameworks**: Bonus points for flexibility

### What Do These Scores Mean?

**Inference Score** (ability to run models):
- Primarily based on Memory + GPU + Frameworks
- Higher score = larger models you can run
- 80+ means you can comfortably run 13B+ models

**Training Score** (ability to train models):
- Requires high GPU score + sufficient RAM
- Most users won't train, fine-tuning is more common
- 70+ needed for practical fine-tuning

**Fine-tuning Score** (ability to adapt models):
- Mid-range hardware is often sufficient
- LoRA/QLoRA techniques reduce requirements
- 60+ enables fine-tuning with modern techniques

---

## Model Recommendations

### How Recommendations Work

The app analyzes your hardware and suggests models based on:

1. **Memory Requirements**: Model fits in your RAM/VRAM
2. **Quantization Support**: 4-bit/8-bit versions for limited VRAM
3. **Expected Performance**: Estimated tokens/second > 5 (usable)
4. **Framework Compatibility**: Available acceleration matches model needs

### Understanding Model Cards

Each recommended model shows:

**Model Name**: Official model identifier
- Example: "Llama 3.2 3B" = Meta's Llama 3.2 with 3 billion parameters

**Description**: What the model is good for
- General chat, coding, reasoning, creative writing, etc.

**Parameter Size Badge**: Model size
- **1B-3B**: Very fast, basic capabilities
- **7B**: Standard, good all-around performance
- **8B-14B**: Enhanced capabilities, slower inference
- **30B-34B**: Advanced, requires high-end hardware
- **70B+**: Top-tier, enthusiast-grade hardware needed

**Performance Estimate**: Expected tokens/second
- **50+ tok/s**: Very fast, real-time chat
- **20-50 tok/s**: Fast, smooth conversation
- **10-20 tok/s**: Moderate, acceptable latency
- **5-10 tok/s**: Slow but usable
- **<5 tok/s**: Too slow for comfortable use

**Compatibility Score**: How well model matches your system
- **90-100%**: Perfect fit, excellent performance
- **80-89%**: Good fit, minor compromises
- **70-79%**: Acceptable, some limitations
- **<70%**: Suboptimal, consider smaller variant

### Quantization Levels

Models come in different precision levels:

- **FP32** (Full Precision): Original quality, 4x memory
- **FP16** (Half Precision): Minimal quality loss, 2x memory
- **8-bit**: Good quality, 4x less memory than FP32
- **4-bit**: Small quality loss, 8x less memory than FP32

Lower precision = fits in less VRAM = runs on more hardware

### Finding More Models

The app includes 50+ popular models in its database:

- **Llama Family**: Meta's open models (1B, 3B, 8B, 70B)
- **Mistral**: Efficient 7B models
- **Gemma**: Google's compact models
- **Phi**: Microsoft's small but capable models
- **DeepSeek**: Coding-focused models
- **Qwen**: Alibaba's multilingual models

Want a model not listed? See [Adding Models Guide](ADDING_MODELS.md).

---

## Upgrade Advisor

### Understanding Bottlenecks

Your **Primary Bottleneck** is the component limiting LLM performance most:

- **GPU/VRAM**: Can't fit larger models, slow inference
- **RAM**: Can't load models into memory
- **Storage**: Slow model loading from disk
- **CPU**: Slow CPU-based inference
- **Frameworks**: No GPU acceleration available

### Upgrade Priority

The Upgrade Suggestions section shows which components need attention:

**Color Coding**:
- **Red** (0-59): Urgent - Major performance impact
- **Orange** (60-79): Moderate - Noticeable improvement possible
- **Green** (80-100): Good - No upgrade needed

**Focus Order**:
1. Fix your Primary Bottleneck first
2. Address any red scores
3. Improve orange scores if budget allows
4. Green scores are fine

### Specific Recommendations

**CPU Upgrades**:
- **Budget** ($200-300): Ryzen 5 5600, Intel i5-12400
- **Mid-Range** ($300-500): Ryzen 7 5800X, Intel i7-13700
- **High-End** ($500+): Ryzen 9 7950X, Intel i9-14900K

**GPU Upgrades**:
- **Budget 8GB** ($400-600): RTX 4060 Ti, RX 7600 XT
- **Mid-Range 12GB** ($600-900): RTX 4070, RX 7700 XT
- **High-End 16GB** ($1000-1600): RTX 4070 Ti Super, RTX 4080
- **Enthusiast 24GB** ($2000+): RTX 4090

**RAM Upgrades**:
- **Minimum**: 32GB (2x16GB DDR4/DDR5)
- **Recommended**: 64GB (2x32GB)
- **Enthusiast**: 128GB (4x32GB or 2x64GB)

**Storage Upgrades**:
- **Budget**: SATA SSD (500GB+)
- **Recommended**: NVMe PCIe 3.0 (1TB+)
- **High-End**: NVMe PCIe 4.0 (2TB+)
- **Enthusiast**: NVMe PCIe 5.0 (2TB+)

**Framework Installation**:
- **NVIDIA GPU**: Install CUDA Toolkit 12.x
- **AMD GPU**: Install ROCm (Linux) or use DirectML (Windows)
- **Windows**: DirectML comes with Windows 10/11
- **macOS**: Metal comes pre-installed

### Before and After Comparison

When considering an upgrade, think about:

**Current State**: What models can you run now?
- Example: 16GB RAM = max 7B models with 4-bit quantization

**After Upgrade**: What will the upgrade enable?
- Example: 32GB RAM = 13B models with 4-bit, 7B models with FP16

**Cost vs Benefit**: Is the improvement worth the cost?
- Example: $100 RAM upgrade unlocks 2x larger models = good value
- Example: $1500 GPU upgrade for 20% speed boost = poor value

---

## Component Details

Click any component score on the Dashboard to see detailed information.

### CPU Details View

**Specifications**:
- Model name and manufacturer
- Core count (physical cores, logical threads)
- Base clock and boost clock speeds
- Cache sizes (L1, L2, L3)
- Architecture and generation
- TDP (power consumption)

**Performance Metrics**:
- Single-core score (model loading, some inference)
- Multi-core score (batch processing, training)
- Memory bandwidth
- Instruction sets (AVX2, AVX-512)

**LLM Impact**:
- **Model Loading**: Affected by single-core speed
- **CPU Inference**: Requires 8+ cores for good performance
- **Quantization**: AVX2/AVX-512 speeds up dequantization

**Upgrade Paths**:
- Specific CPU recommendations based on your current hardware
- Expected performance improvement
- Compatibility notes (socket type, motherboard support)

### GPU Details View

**Specifications**:
- Model name and vendor
- VRAM capacity and type
- CUDA cores / Stream processors / Compute units
- Memory bandwidth
- TDP and power requirements

**Compute Capabilities**:
- CUDA compute capability (NVIDIA)
- GCN/RDNA architecture (AMD)
- DirectX/Vulkan support
- Ray tracing cores (if applicable)

**LLM Impact**:
- **VRAM**: Determines max model size
- **Memory Bandwidth**: Affects inference speed
- **Compute Power**: Determines tokens/second

**Upgrade Paths**:
- GPU recommendations by VRAM tier (8GB, 12GB, 16GB, 24GB)
- Power supply requirements
- Physical dimensions for your case

### RAM Details View

**Specifications**:
- Total capacity
- Number of sticks and configuration
- Memory type (DDR4, DDR5)
- Speed (MT/s or MHz)
- Dual channel vs single channel

**Usage Information**:
- Currently used RAM
- Available RAM
- Committed RAM
- Peak usage

**LLM Impact**:
- **Capacity**: Determines model size (RAM = parameter count × precision bytes)
- **Speed**: Affects CPU inference performance
- **Channels**: Dual channel = 2x bandwidth

**Upgrade Paths**:
- How much RAM to add
- Compatible memory specs
- Expected model size improvements

### Storage Details View

**Specifications**:
- Drive type (HDD, SATA SSD, NVMe SSD)
- Total capacity
- Available free space
- Interface (SATA, NVMe PCIe 3.0/4.0/5.0)

**Performance Metrics**:
- Sequential read speed
- Sequential write speed
- Random read/write IOPS
- Latency

**LLM Impact**:
- **Type**: NVMe loads models in seconds, HDD in minutes
- **Space**: Each model is 5-100GB depending on size
- **Speed**: Faster storage = faster model switching

**Upgrade Paths**:
- NVMe SSD recommendations
- M.2 slot compatibility
- Storage expansion options

### Frameworks Details View

**Detected Frameworks**:
- CUDA: Version, compute capability, compatible GPUs
- ROCm: Version, supported AMD GPUs
- DirectML: Windows version, compatibility
- Metal: macOS version, GPU support
- OpenVINO: Version, Intel optimization

**Missing Frameworks**:
- What's not installed
- Why it matters
- Installation instructions

**LLM Impact**:
- **GPU Acceleration**: Frameworks enable GPU usage
- **Performance**: 10-100x faster than CPU-only
- **Compatibility**: Different models need different frameworks

**Installation Guides**:
- Step-by-step framework installation
- Platform-specific instructions
- Verification steps

---

## Benchmark Testing

### Why Benchmark?

Benchmarks test your actual system performance, not just specifications:

- Verify hardware is working correctly
- Compare against other systems
- Identify performance bottlenecks
- Measure impact of upgrades

### Running a Benchmark

1. Click **Run Benchmark** button on Dashboard
2. Wait 30-60 seconds (do not use other applications)
3. View results in popup modal

**What's Tested**:
- CPU single-core performance
- CPU multi-core performance
- Memory bandwidth
- Estimated tokens/second for different model sizes

### Understanding Benchmark Results

**CPU Single-Core Score**:
- Measures single-threaded performance
- Affects: Model loading speed
- Typical: 800-1200 (mid-range), 1200-1800 (high-end)

**CPU Multi-Core Score**:
- Measures parallel processing power
- Affects: Batch inference, training
- Typical: 5000-10000 (mid-range), 10000-25000 (high-end)

**Memory Bandwidth**:
- Measured in GB/s
- Affects: Data transfer speed to CPU/GPU
- Typical: 30-50 GB/s (DDR4), 60-100 GB/s (DDR5)

**Token Throughput Estimates**:
- Shows estimated tokens/second for 7B, 13B, 34B, 70B models
- Based on your benchmark results
- Actual performance may vary by model and quantization

**Comparison to Reference**:
- Percentage vs. reference system (e.g., "87.3% of reference")
- Reference: Mid-range system (Ryzen 5 5600, 32GB RAM, RTX 3060)
- >100% = faster than reference, <100% = slower

### Benchmark Best Practices

**Before Benchmarking**:
- Close all other applications
- Plug in laptop (don't run on battery)
- Ensure adequate cooling
- Wait for system to idle (low CPU usage)

**After Benchmarking**:
- Save results (automatically saved with report export)
- Compare before/after upgrades
- Note any anomalies or warnings

---

## Export Reports

### Exporting Your Analysis

Save your hardware analysis to a file:

1. Click **Export Report** on Dashboard
2. Choose save location
3. File saved as `LLM_Capability_Report_YYYYMMDD_HHMMSS.json`

### Report Contents

The JSON report includes:

**Hardware Information**:
- Complete CPU specifications
- GPU details and VRAM
- Memory configuration
- Storage information
- Detected frameworks

**System Scores**:
- Overall score
- Component breakdown (CPU, GPU, RAM, Storage, Framework)
- System tier and rating
- Primary bottleneck

**Model Recommendations**:
- Top compatible models
- Compatibility scores
- Performance estimates

**Benchmark Results** (if run):
- CPU scores
- Memory bandwidth
- Token throughput estimates
- Comparison to reference

**Metadata**:
- Report timestamp
- App version
- Operating system

### Using Exported Reports

**Share with Community**:
- Post anonymized reports in discussions
- Help build community database
- Compare with similar systems

**Track Over Time**:
- Export before upgrades
- Export after upgrades
- Compare improvement

**Technical Support**:
- Attach to bug reports
- Share for troubleshooting
- Verify hardware detection accuracy

---

## Settings & Preferences

Access Settings via the Settings button (top-right, gear icon).

### Available Settings

**Display Options**:
- **Theme**: Light or Dark mode (coming soon)
- **Font Size**: Adjust text size for accessibility
- **Animations**: Enable/disable UI animations

**Detection Settings**:
- **Auto-Refresh**: Refresh hardware on app launch
- **Detection Timeout**: How long to wait for hardware detection
- **Detailed Logging**: Enable for troubleshooting

**Model Database**:
- **Update on Startup**: Download latest models.json
- **Cache Models**: Store model database locally
- **Show All Models**: Include experimental models

**Benchmark Settings**:
- **Benchmark Duration**: Quick (30s) or Comprehensive (60s)
- **Share Anonymous Results**: Contribute to community database
- **Save Benchmark History**: Keep past benchmark results

**Privacy**:
- **Telemetry**: None by default (100% local)
- **Crash Reports**: Opt-in anonymous crash reporting
- **Usage Statistics**: Opt-in feature usage stats

### Resetting Settings

Click **Reset to Defaults** to restore all settings to original values.

---

## Troubleshooting

### Common Issues

#### "Hardware Detection Failed"

**Symptoms**: Error message on startup, no hardware shown

**Solutions**:
1. **Run as Administrator** (Windows) or with `sudo` (Linux)
2. Check required tools are installed:
   - Windows: WMI service running
   - Linux: `lscpu`, `lspci`, `lsblk` installed
   - macOS: `sysctl`, `system_profiler` available
3. Check antivirus isn't blocking system queries
4. Try clicking **Refresh** button
5. Check logs in Settings → Enable Detailed Logging

#### "Score is Lower Than Expected"

**Symptoms**: Your hardware seems good but score is low

**Solutions**:
1. Check **Primary Bottleneck** - often one weak component
2. Verify frameworks are installed (GPU score heavily weighted)
3. Check RAM configuration (single vs dual channel)
4. Run benchmark to verify actual performance
5. Update GPU drivers

#### "Benchmark Keeps Failing"

**Symptoms**: Benchmark crashes or shows error

**Solutions**:
1. Close all other applications
2. Ensure sufficient free RAM (4GB+ recommended)
3. Disable overclocking temporarily
4. Update graphics drivers
5. Try "Quick Benchmark" instead of "Comprehensive"

#### "GPU Not Detected"

**Symptoms**: Shows "No dedicated GPU" despite having one

**Solutions**:
1. **Windows**:
   - Update GPU drivers from manufacturer website
   - Check Device Manager for GPU
   - Ensure GPU is enabled in BIOS
2. **Linux**:
   - Install `lspci` package
   - Install nvidia-smi (NVIDIA) or rocm-smi (AMD)
   - Check `lspci | grep VGA` output
3. **macOS**:
   - Run `system_profiler SPDisplaysDataType`
   - Integrated GPUs shown as "Metal" support
4. For laptops: Ensure using dedicated GPU not integrated

#### "Models Don't Match My Hardware"

**Symptoms**: Recommended models seem wrong

**Solutions**:
1. Click **Refresh** to re-detect hardware
2. Check individual component scores - one low score limits recommendations
3. Verify RAM/VRAM amounts are correct
4. Install GPU acceleration frameworks (CUDA, DirectML, etc.)
5. Check for model database updates in Settings

#### "App Runs Slowly"

**Symptoms**: UI is laggy or unresponsive

**Solutions**:
1. Close background applications
2. Disable animations in Settings
3. Check CPU usage (Task Manager / Activity Monitor)
4. Update to latest app version
5. Try reducing font size in Settings

### Error Codes

**ERR_HW_001**: Hardware detection timeout
- Solution: Increase timeout in Settings, check system tools

**ERR_HW_002**: Insufficient permissions
- Solution: Run as Administrator (Windows) or with elevated privileges

**ERR_DB_001**: Model database download failed
- Solution: Check internet connection, try again later

**ERR_BM_001**: Benchmark initialization failed
- Solution: Close other apps, ensure sufficient RAM

**ERR_EXP_001**: Report export failed
- Solution: Check disk space, verify write permissions

### Getting Help

If issues persist:

1. **Enable Detailed Logging**: Settings → Detailed Logging
2. **Export Report**: Capture your system state
3. **Check Logs**: Look for error details
4. **Search Issues**: [GitHub Issues](https://github.com/yourusername/llm-capability-checker/issues)
5. **Report Bug**: Create new issue with logs and report attached

---

## FAQ

### General Questions

**Q: Is this app free?**
A: Yes, completely free and open-source under MIT License.

**Q: Does it send my data anywhere?**
A: No. 100% local operation by default. Telemetry is opt-in only.

**Q: What operating systems are supported?**
A: Windows 10/11, Linux (Ubuntu, Debian, Arch, Fedora), macOS 10.13+

**Q: Do I need an internet connection?**
A: Only for downloading the model database on first run. After that, fully offline.

### Hardware Questions

**Q: Why is my score low despite good hardware?**
A: Check your Primary Bottleneck. One weak component (often VRAM or RAM) can limit overall score.

**Q: What if I don't have a GPU?**
A: CPU-only inference is possible with smaller models (7B and under) and quantization (4-bit).

**Q: Can I run 70B models on my system?**
A: You need 64GB+ RAM (CPU) or 48GB+ VRAM (GPU) for 70B models, even with 4-bit quantization.

**Q: How accurate is hardware detection?**
A: Very accurate on modern systems. If something looks wrong, use the Refresh button or check logs.

**Q: Does Mac M1/M2/M3 support GPU acceleration?**
A: Yes! Apple Silicon Macs use Metal for GPU acceleration, often faster than equivalent PC hardware.

### Model Questions

**Q: Which models can I run?**
A: See Dashboard → Recommended Models. Generally: RAM (GB) / 2 = max model size (B) with 4-bit quantization.

**Q: What's the difference between 7B and 13B models?**
A: More parameters = better quality but slower. 13B models are ~2x larger and ~2x slower than 7B.

**Q: Can I add custom models?**
A: Not yet in UI, but you can edit `data/models.json` directly. Feature coming soon.

**Q: What is quantization?**
A: Reducing model precision (32-bit → 4-bit) to use less memory with minimal quality loss.

### Upgrade Questions

**Q: What should I upgrade first?**
A: Your Primary Bottleneck. Usually RAM (if <32GB) or GPU VRAM (if <12GB).

**Q: How much do upgrades cost?**
A: Budget: $100-300 (RAM/Storage), Mid: $500-1000 (GPU), High: $2000+ (flagship GPU).

**Q: Will upgrading my GPU help if I have 16GB RAM?**
A: Limited benefit. With 16GB RAM, focus on RAM upgrade first (to 32GB), then GPU.

**Q: Is NVMe really necessary?**
A: Not necessary, but highly recommended. NVMe loads models 10x faster than HDD, 3x faster than SATA SSD.

### Technical Questions

**Q: How are compatibility scores calculated?**
A: Based on memory requirements, framework support, expected performance, and hardware benchmarks.

**Q: What's the difference between Inference, Training, and Fine-tuning scores?**
A: Inference (running models) needs RAM/VRAM. Training needs powerful GPU. Fine-tuning is in between.

**Q: Why does the benchmark take so long?**
A: Testing CPU, memory, and estimating throughput accurately requires 30-60 seconds of sustained load.

**Q: Can I benchmark my GPU?**
A: Not yet. Current benchmarks focus on CPU and memory. GPU benchmarking coming in future update.

---

## Keyboard Shortcuts

- **F1**: Open Help
- **F5**: Refresh Dashboard
- **Ctrl+E**: Export Report (Windows/Linux)
- **Cmd+E**: Export Report (macOS)
- **Ctrl+,**: Open Settings (Windows/Linux)
- **Cmd+,**: Open Settings (macOS)
- **Escape**: Close modals/dialogs

---

## Additional Resources

### Documentation
- [FAQ](FAQ.md) - Frequently Asked Questions
- [Technical Requirements](../TECHNICAL_REQUIREMENTS.md) - Detailed specifications
- [Architecture](../ARCHITECTURE.md) - System design
- [Cross-Platform Testing](CROSS_PLATFORM_TESTING.md) - Platform-specific details

### Community
- [GitHub Discussions](https://github.com/yourusername/llm-capability-checker/discussions) - Ask questions
- [GitHub Issues](https://github.com/yourusername/llm-capability-checker/issues) - Report bugs
- [Contributing Guide](../CONTRIBUTING.md) - Help improve the app

### External Resources
- [Hugging Face Model Hub](https://huggingface.co/models) - Find LLM models
- [llama.cpp](https://github.com/ggerganov/llama.cpp) - Run models locally
- [Ollama](https://ollama.ai/) - Easy local LLM deployment
- [r/LocalLLaMA](https://reddit.com/r/LocalLLaMA) - Community discussions

---

## About This Guide

**Version**: 1.0.0
**Last Updated**: 2025-10-15
**Feedback**: Found an error or have suggestions? [Open an issue](https://github.com/yourusername/llm-capability-checker/issues)

---

**Need more help?** Press **F1** in the app for context-sensitive help, or visit our [GitHub Discussions](https://github.com/yourusername/llm-capability-checker/discussions).
