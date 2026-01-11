import 'package:flutter/material.dart';

void main() => runApp(const ConverterApp());

/// A small Flutter app that converts between metric and imperial units.
///
/// Technical notes:
/// - Uses a base unit approach for conversions (distance -> meters, weight -> grams)
/// - Conversion is `value * (fromFactor / toFactor)` where factors are relative to base unit
/// - UI: dropdowns to select category, from/to units, text field for input, and a convert button
class ConverterApp extends StatelessWidget {
  const ConverterApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Unit Converter',
      theme: ThemeData(primarySwatch: Colors.blue),
      home: const ConverterHome(),
    );
  }
}

class ConverterHome extends StatefulWidget {
  const ConverterHome({super.key});

  @override
  State<ConverterHome> createState() => _ConverterHomeState();
}

class _ConverterHomeState extends State<ConverterHome> {
  // Unit factors relative to base units: distance -> meters, weight -> grams
  static const Map<String, Map<String, double>> unitFactors = {
    'Distance': {
      'Meters': 1.0,
      'Kilometers': 1000.0,
      'Miles': 1609.344,
      'Yards': 0.9144,
    },
    'Weight': {
      'Grams': 1.0,
      'Kilograms': 1000.0,
      'Pounds': 453.59237,
      'Ounces': 28.349523125,
    },
  };

  String _category = 'Distance';
  String _fromUnit = 'Meters';
  String _toUnit = 'Kilometers';
  final TextEditingController _controller = TextEditingController();
  String _resultText = '';

  @override
  void initState() {
    super.initState();
    // Initialize default units based on category
    _fromUnit = unitFactors[_category]!.keys.first;
    _toUnit = unitFactors[_category]!.keys.elementAt(1);
  }

  void _onCategoryChanged(String? newCategory) {
    if (newCategory == null) return;
    setState(() {
      _category = newCategory;
      final keys = unitFactors[_category]!.keys.toList();
      _fromUnit = keys.first;
      _toUnit = keys.length > 1 ? keys[1] : keys.first;
      _resultText = '';
      _controller.clear();
    });
  }

  void _convert() {
    final input = _controller.text.trim();
    if (input.isEmpty) {
      setState(() => _resultText = 'Enter a value to convert.');
      return;
    }

    final value = double.tryParse(input);
    if (value == null) {
      setState(() => _resultText = 'Invalid number format.');
      return;
    }

    final fromFactor = unitFactors[_category]![_fromUnit]!;
    final toFactor = unitFactors[_category]![_toUnit]!;

    final result = value * (fromFactor / toFactor);

    setState(() {
      _resultText = '${value.toString()} $_fromUnit = ${result.toStringAsFixed(4)} $_toUnit';
    });
  }

  @override
  Widget build(BuildContext context) {
    final units = unitFactors[_category]!.keys.toList();

    return Scaffold(
      appBar: AppBar(title: const Text('Unit Converter')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Category selector
            Row(
              children: [
                const Text('Measure:'),
                const SizedBox(width: 12),
                DropdownButton<String>(
                  value: _category,
                  items: unitFactors.keys
                      .map((k) => DropdownMenuItem(value: k, child: Text(k)))
                      .toList(),
                  onChanged: _onCategoryChanged,
                ),
              ],
            ),
            const SizedBox(height: 16),

            // From / To selectors
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('From'),
                      DropdownButton<String>(
                        value: _fromUnit,
                        isExpanded: true,
                        items: units
                            .map((u) => DropdownMenuItem(value: u, child: Text(u)))
                            .toList(),
                        onChanged: (v) => setState(() => _fromUnit = v ?? _fromUnit),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 12),
                const Icon(Icons.swap_horiz),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('To'),
                      DropdownButton<String>(
                        value: _toUnit,
                        isExpanded: true,
                        items: units
                            .map((u) => DropdownMenuItem(value: u, child: Text(u)))
                            .toList(),
                        onChanged: (v) => setState(() => _toUnit = v ?? _toUnit),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Input
            TextField(
              controller: _controller,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: const InputDecoration(
                border: OutlineInputBorder(),
                labelText: 'Value to convert',
              ),
              onSubmitted: (_) => _convert(),
            ),
            const SizedBox(height: 12),

            ElevatedButton(
              onPressed: _convert,
              child: const Text('Convert'),
            ),
            const SizedBox(height: 20),

            // Result
            Text(
              _resultText,
              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            const Spacer(),

            const Text(
              'Notes: This app demonstrates metric/imperial conversion.',
              style: TextStyle(fontSize: 12),
            ),
          ],
        ),
      ),
    );
  }
}
