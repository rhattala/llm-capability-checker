# Example Reports

This directory contains sample output reports from the LLM Capability Checker to help you understand what to expect.

## Files

### `sample-report.txt`
Complete system report example showing:
- Hardware configuration detection
- Three capability scores (Inference, Training, Fine-Tuning)
- Model compatibility breakdown (68/107 models)
- Specific upgrade recommendations
- Community insights

**Sample Hardware**: AMD Ryzen 7 5800X + RTX 4070 Ti 12GB + 32GB RAM

This represents a typical high-end consumer setup in 2025.

## Generating Your Own Report

Run the application and use the "Export Report" button on the dashboard to generate a report for your system.

Export formats available:
- Plain Text (.txt)
- Markdown (.md)
- JSON (.json)
- HTML (.html)

## What Makes a Good Report

The reports are designed to be:
- **Actionable**: Specific recommendations, not vague advice
- **Honest**: Shows failures and limitations, not just successes
- **Comparative**: Your hardware vs reference systems
- **Educational**: Explains why certain models work or don't

## Contributing Example Reports

If you have an interesting hardware configuration and want to share your report:

1. Export your report (anonymize any personal info)
2. Add it to this directory with a descriptive name: `report-rtx4090-64gb.txt`
3. Update this README with a brief description
4. Submit a PR

Particularly useful configurations:
- Budget builds (GTX 1660, RTX 3060, etc.)
- High-end workstations (RTX 4090, A6000, etc.)
- AMD GPU setups (7900 XTX with ROCm)
- Apple Silicon (M1/M2/M3 with Metal)
- Multi-GPU configurations
