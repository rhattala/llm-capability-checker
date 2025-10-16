# Frequently Asked Questions (FAQ)

Quick answers to common questions about LLM Capability Checker.

## Table of Contents

- [General Questions](#general-questions)
- [Scores & Ratings](#scores--ratings)
- [Hardware & Detection](#hardware--detection)
- [Models & Compatibility](#models--compatibility)
- [Upgrades & Recommendations](#upgrades--recommendations)
- [Performance & Benchmarks](#performance--benchmarks)
- [Privacy & Data](#privacy--data)
- [Troubleshooting](#troubleshooting)
- [Platform-Specific](#platform-specific)

---

## General Questions

### What is LLM Capability Checker?

LLM Capability Checker is a free, open-source desktop application that analyzes your computer hardware and tells you:
- Which Large Language Models you can run locally
- How fast they'll perform on your system
- What hardware upgrades would help most
- Your system's overall capability score (0-100)

### Is it free?

Yes, completely free and open-source under the MIT License. No subscriptions, no ads, no hidden costs.

### What operating systems are supported?

- **Windows**: 10 and 11 (64-bit)
- **Linux**: Ubuntu, Debian, Arch, Fedora, and other major distributions (x64)
- **macOS**: 10.13+ (High Sierra or later), both Intel and Apple Silicon (M1/M2/M3)

### Do I need to install anything else?

Only the .NET 8.0 Runtime (if not already installed). On most modern systems, this is already present or will be installed automatically.

### How much space does it take?

- **Application**: ~50MB
- **Runtime Requirements**: ~100MB additional RAM while running
- **Exports**: JSON reports are typically 10-50KB

---

## Scores & Ratings

### Why is my score low?

Your score reflects your system's capability to run LLM models. Common reasons for low scores:

1. **Insufficient RAM**: Less than 32GB limits you to smaller models
2. **No Dedicated GPU**: Integrated graphics get low GPU scores
3. **Low VRAM**: GPUs with <12GB VRAM limit model sizes
4. **HDD Storage**: Hard drives are very slow for model loading
5. **Missing Frameworks**: No CUDA/DirectML means no GPU acceleration

Check your **Primary Bottleneck** on the Dashboard to see what's limiting you most.

### What do the score ranges mean?

**Overall Score**:
- **80-100** (Excellent): Can run most models smoothly, including 13B-34B parameter models
- **60-79** (Good): Can run common 7B-13B models comfortably
- **40-59** (Fair): Limited to smaller models (1B-7B) with quantization
- **20-39** (Limited): Only tiny models (1B-3B) with heavy quantization
- **0-19** (Poor): Significant upgrades needed for any practical use

### How is the overall score calculated?

Weighted average of component scores:
- **GPU Score**: 30% (most important for inference speed)
- **Memory (RAM) Score**: 25% (determines model size you can load)
- **CPU Score**: 20% (affects loading and CPU inference)
- **Storage Score**: 15% (affects model loading time)
- **Framework Score**: 10% (enables GPU acceleration)

### My hardware is good but my score is low. Why?

Check these common issues:

1. **Missing GPU Drivers**: Update to latest from manufacturer
2. **No ML Frameworks**: Install CUDA (NVIDIA) or DirectML (Windows)
3. **Single Channel RAM**: Using 1 stick instead of 2 (dual channel is 2x faster)
4. **Integrated GPU**: Laptop using power-saving mode, not dedicated GPU
5. **Outdated Drivers**: Old drivers can cause hardware to be underutilized

Click the component with the lowest score for specific recommendations.

---

## Hardware & Detection

### How accurate is hardware detection?

Very accurate on modern systems (2015+). The app uses:
- **Windows**: WMI (Windows Management Instrumentation)
- **Linux**: Standard command-line tools (lscpu, lspci, etc.)
- **macOS**: System profiler and sysctl

Detection accuracy: ~95% for common hardware, lower for exotic/custom setups.

### What if my hardware isn't detected correctly?

1. Click **Refresh** button on Dashboard
2. Run the app with elevated permissions (Administrator/sudo)
3. Check that system tools are installed:
   - Linux: `sudo apt install lshw pciutils util-linux` (Ubuntu/Debian)
   - macOS: System tools are pre-installed
4. Update your hardware drivers
5. [Report the issue](https://github.com/yourusername/llm-capability-checker/issues) with your system specs

### Why doesn't it detect my GPU?

**For NVIDIA GPUs**:
- Install latest drivers from [nvidia.com](https://nvidia.com/drivers)
- Install CUDA Toolkit (optional, but helps with detection)
- On Linux, install `nvidia-smi`: `sudo apt install nvidia-utils`

**For AMD GPUs**:
- Install latest drivers from [amd.com](https://amd.com/support)
- On Linux, install `rocm-smi` if using ROCm
- On Windows, DirectML should detect automatically

**For Intel GPUs**:
- Usually detected as integrated graphics
- Update Intel graphics drivers
- OpenVINO toolkit helps with detection

**For laptops**: Ensure you're using the dedicated GPU, not integrated graphics. Check power settings.

### What if I don't have a dedicated GPU?

You can still run smaller LLM models (1B-7B) using CPU-only inference:

- **Pros**: Works on any system, no extra hardware needed
- **Cons**: 10-100x slower than GPU inference
- **Recommended**: 8+ core CPU, 32GB+ RAM, use 4-bit quantized models
- **Models**: Phi-3 Mini (3.8B), Llama 3.2 (3B), TinyLlama (1.1B)

Your GPU score will be low (~30-40), but other scores can compensate.

### Can I run this on a laptop?

Yes! Many laptops have sufficient hardware for 7B-13B models:

- **Minimum**: 16GB RAM, any CPU, integrated graphics → Small models (3B-7B)
- **Good**: 32GB RAM, dedicated GPU with 6-8GB VRAM → Medium models (7B-13B)
- **Great**: 64GB RAM, RTX 4070/4080 Mobile → Large models (13B-34B)

Gaming laptops typically score 60-80, while MacBook Pro M-series score 70-90.

---

## Models & Compatibility

### Which models can I run?

Check the **Recommended Models** section on your Dashboard. Generally:

**Rule of Thumb**: Your RAM (GB) divided by 2 ≈ max model size (billions of parameters) with 4-bit quantization.

Examples:
- **16GB RAM**: 7B models (4-bit quantization)
- **32GB RAM**: 13B models (4-bit) or 7B models (full precision)
- **64GB RAM**: 34B models (4-bit) or 13B models (full precision)
- **128GB RAM**: 70B models (4-bit) or 34B models (full precision)

### What does "7B" or "13B" mean?

The number represents billions of parameters (model size):

- **1B-3B**: Tiny models, basic capabilities, very fast
- **7B**: Standard size, good all-around performance
- **13B-14B**: Enhanced capabilities, moderate speed
- **30B-34B**: Advanced capabilities, slower but high quality
- **70B+**: Top-tier quality, requires enthusiast hardware

More parameters = better quality but slower performance and higher hardware requirements.

### What is quantization?

Quantization reduces model precision to use less memory:

- **FP32** (32-bit): Full precision, original quality, ~4 bytes per parameter
- **FP16** (16-bit): Half precision, minimal quality loss, ~2 bytes per parameter
- **8-bit**: Good quality, ~1 byte per parameter (4x less memory than FP32)
- **4-bit**: Small quality loss, ~0.5 bytes per parameter (8x less memory than FP32)

**Example**: A 7B model at FP32 needs ~28GB RAM, but only ~3.5GB at 4-bit.

Lower quantization = fits in less memory = runs on weaker hardware, but with slight quality trade-off.

### Can I run 70B models on my system?

70B models are very demanding:

**Minimum Requirements**:
- **CPU-only**: 64GB+ RAM (4-bit quantization), 10+ core CPU
  - Performance: 1-3 tokens/second (very slow)
- **GPU**: 48GB+ VRAM (RTX 6000 Ada, A6000) or 2x RTX 4090 (48GB combined)
  - Performance: 20-50 tokens/second (usable)

**Reality**: Most users should focus on 7B-34B models. 70B models are for enthusiasts with high-end hardware.

### What about training or fine-tuning?

**Training from scratch**: Not practical for individuals, requires massive compute clusters.

**Fine-tuning existing models**:
- **LoRA/QLoRA**: Efficient fine-tuning, works on consumer hardware
- **Requirements**: 16GB+ VRAM (GPU), 32GB+ RAM
- **Models**: Can fine-tune 7B-13B models on single RTX 4070/4080
- **Time**: Hours to days depending on dataset size

The app's Fine-tuning Score estimates your ability to fine-tune using LoRA techniques.

### How do I actually run a model after checking compatibility?

This app only checks compatibility. To actually run models, use:

1. **[Ollama](https://ollama.ai/)**: Easiest, one-command install
2. **[LM Studio](https://lmstudio.ai/)**: User-friendly GUI
3. **[llama.cpp](https://github.com/ggerganov/llama.cpp)**: Command-line, most flexible
4. **[GPT4All](https://gpt4all.io/)**: Simple desktop app
5. **[Text Generation WebUI](https://github.com/oobabooga/text-generation-webui)**: Advanced, feature-rich

Download models from [Hugging Face](https://huggingface.co/models).

---

## Upgrades & Recommendations

### What should I upgrade first?

Always upgrade your **Primary Bottleneck** first (shown on Dashboard):

**If bottleneck is RAM**:
- Upgrade to 32GB minimum (2x16GB dual channel)
- Biggest impact for model compatibility
- Cost: $50-150 depending on DDR4 vs DDR5

**If bottleneck is GPU/VRAM**:
- Upgrade to 12GB+ VRAM GPU
- Biggest impact for inference speed
- Cost: $400-1500 depending on model

**If bottleneck is Storage**:
- Upgrade to NVMe SSD
- Biggest impact for model loading time
- Cost: $50-150 for 1TB

**If bottleneck is CPU**:
- Upgrade to 8+ core modern CPU
- Impact: Faster CPU inference and model loading
- Cost: $200-500 (may require new motherboard)

**If bottleneck is Frameworks**:
- Install CUDA, DirectML, or ROCm (free)
- Enables GPU acceleration
- Cost: $0

### How much do the recommended upgrades cost?

**Budget-Friendly Upgrades** ($100-300):
- 32GB RAM (2x16GB DDR4): $50-100
- 1TB NVMe SSD: $50-100
- Framework installation: Free
- **Total**: $100-200 for significant improvement

**Mid-Range Upgrades** ($500-1200):
- RTX 4060 Ti (16GB) or 4070: $600-800
- 64GB RAM: $150-250
- 2TB NVMe PCIe 4.0: $100-150
- **Total**: $850-1200

**High-End Upgrades** ($2000-3000):
- RTX 4080 Super (16GB) or 4090 (24GB): $1200-2000
- 128GB RAM: $300-500
- Modern CPU (Ryzen 9 / i9): $500-700
- **Total**: $2000-3200

### Will upgrading help if I already have decent hardware?

Diminishing returns apply:

- **Low score (0-40)**: Upgrades have huge impact
- **Medium score (40-70)**: Upgrades have good impact
- **High score (70-85)**: Upgrades have moderate impact
- **Very high score (85-100)**: Upgrades have minimal impact

If you're already at 80+, focus on specific needs (more VRAM, faster storage) rather than overall score.

### Should I upgrade RAM or GPU first?

**Upgrade RAM first if**:
- You have <32GB RAM
- Your RAM score is red (<60)
- You can't load the models you want (out of memory errors)
- Your GPU has adequate VRAM (8GB+)

**Upgrade GPU first if**:
- You have 32GB+ RAM already
- Your GPU has <8GB VRAM
- Models load but run very slowly
- You have no dedicated GPU

**Upgrade both if**:
- You have <16GB RAM and no dedicated GPU
- You're starting from scratch

### What if I can't afford upgrades?

Focus on optimizations:

1. **Use smaller models**: 7B instead of 13B
2. **Use heavier quantization**: 4-bit instead of 8-bit
3. **Close background apps**: Free up RAM
4. **Use CPU inference**: Slower but free
5. **Install frameworks**: CUDA/DirectML are free
6. **Upgrade incrementally**: Start with RAM ($100), add GPU later

Even modest systems (16GB RAM, integrated GPU) can run Phi-3 Mini (3.8B) or Llama 3.2 (3B) models.

---

## Performance & Benchmarks

### What does the benchmark test?

The benchmark measures:

1. **CPU Single-Core Performance**: Single-threaded tasks (model loading)
2. **CPU Multi-Core Performance**: Parallel tasks (batch inference)
3. **Memory Bandwidth**: RAM speed (affects CPU inference)
4. **Token Throughput Estimates**: Estimated tokens/second for 7B, 13B, 34B, 70B models

### How long does a benchmark take?

- **Quick Benchmark**: ~30 seconds
- **Comprehensive Benchmark**: ~60 seconds

Close all other applications for most accurate results.

### What's a good benchmark score?

**CPU Single-Core** (typical range: 800-1800):
- **<800**: Below average, consider CPU upgrade
- **800-1200**: Average, sufficient for most use cases
- **1200-1600**: Good, fast model loading
- **1600+**: Excellent, top-tier performance

**CPU Multi-Core** (typical range: 4000-25000):
- **<5000**: Limited parallel processing
- **5000-10000**: Good for 8-12 core CPUs
- **10000-15000**: Great for 16+ core CPUs
- **15000+**: Excellent, enthusiast-grade

**Memory Bandwidth** (typical range: 20-100 GB/s):
- **<30 GB/s**: Slow (old DDR3)
- **30-50 GB/s**: Average (DDR4)
- **50-80 GB/s**: Good (fast DDR4, entry DDR5)
- **80+ GB/s**: Excellent (high-end DDR5)

### Why are my benchmark results lower than expected?

Common causes:

1. **Thermal Throttling**: CPU overheating, slowing down
2. **Power Settings**: Laptop on battery saver mode
3. **Background Processes**: Other apps using CPU/RAM
4. **Single vs Dual Channel RAM**: 2x slower with single channel
5. **Outdated Drivers**: Old firmware limiting performance

Try closing all apps, plugging in laptop, and ensuring good cooling.

---

## Privacy & Data

### Is my data sent anywhere?

**By default: No.** The app operates 100% locally:
- Hardware detection runs on your machine
- Calculations done locally
- No telemetry or tracking
- No account required
- No server communication (except model database download)

### What about the model database download?

On first run, the app downloads `models.json` (~50KB) from GitHub containing:
- Model names and specifications
- Memory requirements
- Performance characteristics
- No personal data involved

This can be disabled in Settings (use cached database).

### Can I opt into sharing data?

Yes, optional opt-in features (disabled by default):

1. **Anonymous Benchmark Sharing**: Share your benchmark scores with the community database
   - No personal info included
   - Helps build performance comparison database
   - Completely optional

2. **Crash Reports**: Send anonymous crash logs
   - Only if app crashes
   - No personal data
   - Helps developers fix bugs

3. **Usage Statistics**: Basic feature usage stats
   - Which features are used
   - No personal data
   - Helps prioritize development

All opt-in via Settings → Privacy.

### Where are my reports stored?

Exported reports are saved to your chosen location:
- Default: Your Documents folder
- Format: JSON text file
- Contains: Hardware specs, scores, recommendations
- You control: Where to save, who to share with

### Can I use this offline?

Yes, after initial setup:
1. First run requires internet (download model database)
2. After that, fully offline
3. Optional: Disable "Update on Startup" in Settings

---

## Troubleshooting

### The app won't start

1. **Check .NET 8.0 Runtime**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Run as Administrator**: Right-click → "Run as Administrator" (Windows)
3. **Check antivirus**: Temporarily disable to test
4. **Check system requirements**: Windows 10+, Linux x64, macOS 10.13+
5. **Check logs**: Enable detailed logging in Settings (if app starts partially)

### Hardware detection shows "Unknown" or fails

**Windows**:
- Run as Administrator
- Check WMI service: `services.msc` → "Windows Management Instrumentation" → Start
- Update Windows

**Linux**:
- Install tools: `sudo apt install lshw pciutils util-linux` (Ubuntu/Debian)
- Run with sudo if needed: `sudo ./LLMCapabilityChecker`
- Check `dmesg` for hardware errors

**macOS**:
- Grant permissions: System Preferences → Security & Privacy
- Update to latest macOS version

### Benchmark keeps failing or crashing

1. **Close all other applications**
2. **Ensure 4GB+ free RAM**
3. **Disable overclocking** temporarily
4. **Update graphics drivers**
5. **Try "Quick Benchmark"** instead of "Comprehensive"
6. **Check cooling**: Ensure CPU isn't overheating

### Scores don't make sense

1. **Click Refresh**: Re-detect hardware
2. **Check component details**: Click each score to see breakdown
3. **Verify hardware**: Compare detected specs to actual specs
4. **Update drivers**: Especially GPU drivers
5. **Run benchmark**: Test actual performance vs estimated

### Export report fails

1. **Check disk space**: Ensure 10MB+ free
2. **Check permissions**: Try saving to Desktop instead of system folders
3. **Close other apps**: File might be locked
4. **Try different format**: JSON export is more reliable

### Can't find specific model in recommendations

The app shows top 3 compatible models. For full list:
- Check the model database: `data/models.json`
- Models not listed aren't in the database yet
- You can [request model additions](https://github.com/yourusername/llm-capability-checker/issues)

---

## Platform-Specific

### Windows

**Q: Do I need to install Visual C++ Redistributables?**
A: Usually included with .NET 8.0 Runtime. If app crashes on startup, install [VC++ Redist](https://aka.ms/vs/17/release/vc_redist.x64.exe).

**Q: Why does Windows Defender flag the app?**
A: False positive on initial release. App is open-source, you can verify code on GitHub.

**Q: Can I run on Windows 7/8?**
A: No, Windows 10+ required due to .NET 8.0 and WMI dependencies.

### Linux

**Q: What Linux distributions are supported?**
A: Ubuntu, Debian, Arch, Fedora, openSUSE, and most major distros (x64 architecture).

**Q: Do I need to install additional packages?**
A: Usually yes: `sudo apt install lshw pciutils util-linux` (Ubuntu/Debian) or equivalent for your distro.

**Q: Can I run on ARM Linux (Raspberry Pi)?**
A: Not currently, x64 only. ARM64 support coming in future update.

**Q: Permission denied errors?**
A: Some hardware detection requires root: `sudo ./LLMCapabilityChecker` or `sudo dotnet run`

### macOS

**Q: Does it support Apple Silicon (M1/M2/M3)?**
A: Yes! Apple Silicon Macs get excellent scores (70-90) due to unified memory and Metal GPU acceleration.

**Q: Intel Mac vs Apple Silicon?**
A: Apple Silicon is typically faster and more efficient. M-series Macs score 10-20 points higher than equivalent Intel Macs.

**Q: Can I run this on older Macs?**
A: macOS 10.13+ required. Older Macs (pre-2015) may have detection issues.

**Q: Why is my VRAM shown as 0GB?**
A: Apple Silicon uses unified memory (shared between CPU and GPU). The app shows total memory in RAM, not separate VRAM.

---

## Still Need Help?

### Quick Links

- **User Guide**: [USER_GUIDE.md](USER_GUIDE.md) - Comprehensive documentation
- **GitHub Issues**: [Report bugs](https://github.com/yourusername/llm-capability-checker/issues)
- **Discussions**: [Ask questions](https://github.com/yourusername/llm-capability-checker/discussions)
- **In-App Help**: Press **F1** in the application

### Before Asking for Help

1. **Check this FAQ**: Most questions answered here
2. **Read User Guide**: Detailed explanations for all features
3. **Enable Detailed Logging**: Settings → Detailed Logging
4. **Export Report**: Capture your system state
5. **Search Existing Issues**: Your question might be answered

### Reporting Bugs

When reporting issues, include:
- Operating system and version
- App version (About → Version)
- Exported report (if possible)
- Detailed log file (if detailed logging enabled)
- Steps to reproduce the issue

---

**Last Updated**: 2025-10-15
**App Version**: 1.0.0

**Found this helpful?** Give us a star on [GitHub](https://github.com/yourusername/llm-capability-checker)!
