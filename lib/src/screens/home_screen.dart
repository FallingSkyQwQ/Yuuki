import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher_string.dart';

import '../auth/services/microsoft_auth_service.dart';
import '../models/account_model.dart';
import '../models/instance_model.dart';
import '../models/java_runtime_model.dart';
import '../models/version_manifest.dart';
import '../state/auth_state.dart';
import '../state/instance_state.dart';
import '../state/java_runtime_state.dart';
import '../state/launcher_state.dart';
import '../state/modrinth_state.dart';
import '../state/version_state.dart';

class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key});

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  final TextEditingController _offlineController = TextEditingController();
  final TextEditingController _littleSkinServerController = TextEditingController(text: 'https://authserver.example.com');
  final TextEditingController _littleSkinUserController = TextEditingController();
  final TextEditingController _littleSkinPasswordController = TextEditingController();
  final TextEditingController _instanceNameController = TextEditingController(text: 'New Instance');
  final TextEditingController _instanceVersionController = TextEditingController(text: '1.20.1');
  final TextEditingController _modrinthSearchController = TextEditingController();
  final TextEditingController _heapController = TextEditingController(text: '4096');
  final TextEditingController _jvmArgsController = TextEditingController(text: '-XX:+UseG1GC,-Xmx4G');
  final TextEditingController _runtimeLabelController = TextEditingController(text: 'Custom Java');
  final TextEditingController _runtimePathController = TextEditingController(text: '/usr/bin/java');
  final TextEditingController _runtimeVersionController = TextEditingController(text: '17');
  String? _selectedVersionId;
  String? _selectedRuntimeId;

  @override
  void dispose() {
    _offlineController.dispose();
    _littleSkinServerController.dispose();
    _littleSkinUserController.dispose();
    _littleSkinPasswordController.dispose();
    _instanceNameController.dispose();
    _instanceVersionController.dispose();
    _modrinthSearchController.dispose();
    _heapController.dispose();
    _jvmArgsController.dispose();
    _runtimeLabelController.dispose();
    _runtimePathController.dispose();
    _runtimeVersionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final launcherState = ref.watch(launcherStateProvider);
    final launcherNotifier = ref.read(launcherStateProvider.notifier);
    final accountsAsync = ref.watch(accountsProvider);
    final deviceCodeState = ref.watch(deviceCodeProvider);
    final deviceCodeNotifier = ref.read(deviceCodeProvider.notifier);
    final offlineText = _offlineController.text;
    final littleSkinServer = _littleSkinServerController.text;
    final littleSkinUser = _littleSkinUserController.text;
    final littleSkinPass = _littleSkinPasswordController.text;
    final littleSkinState = ref.watch(littleSkinAuthProvider);
    final littleSkinNotifier = ref.read(littleSkinAuthProvider.notifier);
    final activeAccount = ref.watch(activeAccountProvider);
    final activeNotifier = ref.read(activeAccountProvider.notifier);
    final instances = ref.watch(instanceManagerProvider);
    final instanceNotifier = ref.read(instanceManagerProvider.notifier);
    final modrinthState = ref.watch(modrinthSearchProvider);
    final modrinthNotifier = ref.read(modrinthSearchProvider.notifier);
    final versionsAsync = ref.watch(versionListProvider);
    final loaderSelection = ref.watch(loaderSelectionProvider);
    final loaderNotifier = ref.read(loaderSelectionProvider.notifier);
    final runtimes = ref.watch(javaRuntimeProvider);
    VersionDescriptor? selectedVersion;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Yuuki Launcher'),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Status: ${launcherState.statusMessage}', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 8),
            Text('Ready: ${launcherState.ready ? '✅' : '⌛'}', style: Theme.of(context).textTheme.bodyLarge),
            const SizedBox(height: 4),
            Text('Last update: ${_formatTimestamp(launcherState.lastUpdated)}', style: Theme.of(context).textTheme.bodySmall),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: launcherNotifier.initializeCore,
              icon: const Icon(Icons.flash_on),
              label: const Text('Initialize Core'),
            ),
            const SizedBox(height: 12),
            ElevatedButton.icon(
              onPressed: launcherNotifier.refreshStatus,
              icon: const Icon(Icons.sync),
              label: const Text('Refresh Status'),
            ),
            const SizedBox(height: 24),
            const Text('Available Accounts', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            accountsAsync.when(
              data: (accounts) {
                if (activeAccount == null && accounts.isNotEmpty) {
                  WidgetsBinding.instance.addPostFrameCallback((_) {
                    if (ref.read(activeAccountProvider) == null) {
                      activeNotifier.select(accounts.first);
                    }
                  });
                }
                return Column(
                  children: accounts
                      .map((account) => _AccountTile(
                            account,
                            isActive: activeAccount?.id == account.id,
                          ))
                      .toList(),
                );
              },
              loading: () => const SizedBox(
                height: 48,
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (error, stack) => Text('Failed to load accounts: $error'),
            ),
            const SizedBox(height: 24),
            const Text('Microsoft Authentication', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            deviceCodeState.when(
              data: (code) => code == null
                  ? const Text('Request a device code to begin.')
                  : _DeviceCodeCard(code: code),
              loading: () => const LinearProgressIndicator(),
              error: (error, stack) => Text('Device login failed: $error'),
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: ElevatedButton.icon(
                    onPressed: () async {
                      await deviceCodeNotifier.requestDeviceCode();
                    },
                    icon: const Icon(Icons.vpn_key),
                    label: const Text('Request Device Code'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () async {
                      final messenger = ScaffoldMessenger.of(context);
                      try {
                        await deviceCodeNotifier.completeLogin();
                        messenger.showSnackBar(const SnackBar(
                          content: Text('Microsoft account linked'),
                        ));
                        ref.invalidate(accountsProvider);
                      } catch (error) {
                        messenger.showSnackBar(SnackBar(
                          content: Text('Failed to link account: $error'),
                        ));
                      }
                    },
                    icon: const Icon(Icons.check),
                    label: const Text('Complete Authorization'),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 24),
            const Text('LittleSkin Authentication', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            littleSkinState.when(
              data: (account) => Text(account == null
                  ? 'Link a LittleSkin server to add a profile.'
                  : 'Linked LittleSkin profile: ${account.username}'),
              loading: () => const SizedBox(height: 40, child: Center(child: LinearProgressIndicator())),
              error: (error, stack) => Text('LittleSkin login failed: $error'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _littleSkinServerController,
              decoration: const InputDecoration(
                labelText: 'LittleSkin Server URL',
                hintText: 'https://authserver.example.com',
              ),
              onChanged: (_) => setState(() {}),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _littleSkinUserController,
              decoration: const InputDecoration(
                labelText: 'Username',
              ),
              onChanged: (_) => setState(() {}),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _littleSkinPasswordController,
              decoration: const InputDecoration(
                labelText: 'Password',
              ),
              obscureText: true,
              onChanged: (_) => setState(() {}),
            ),
            const SizedBox(height: 12),
            ElevatedButton.icon(
              onPressed: littleSkinServer.trim().isEmpty || littleSkinUser.trim().isEmpty || littleSkinPass.trim().isEmpty
                  ? null
                  : () async {
                      final messenger = ScaffoldMessenger.of(context);
                      try {
                        await littleSkinNotifier.authenticate(
                          server: littleSkinServer,
                          username: littleSkinUser,
                          password: littleSkinPass,
                        );
                        messenger.showSnackBar(const SnackBar(content: Text('LittleSkin account linked')));
                        ref.invalidate(accountsProvider);
                      } catch (error) {
                        messenger.showSnackBar(SnackBar(content: Text('LittleSkin login failed: $error')));
                      }
                    },
              icon: const Icon(Icons.cloud),
              label: const Text('Login to LittleSkin'),
            ),
            const SizedBox(height: 24),
            const Text('Offline Accounts', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            TextField(
              controller: _offlineController,
              decoration: const InputDecoration(
                labelText: 'Offline username',
                hintText: 'Enter a display name',
              ),
              onChanged: (_) => setState(() {}),
            ),
            const SizedBox(height: 8),
            ElevatedButton.icon(
              onPressed: offlineText.trim().isEmpty
                  ? null
                  : () async {
                      final repo = ref.read(authRepositoryProvider);
                      final messenger = ScaffoldMessenger.of(context);
                      final trimmed = offlineText.trim();
                      try {
                        final account = await repo.addOfflineAccount(trimmed);
                        messenger.showSnackBar(SnackBar(
                          content: Text('Created offline account ${account.username}'),
                        ));
                        _offlineController.clear();
                        setState(() {});
                        ref.invalidate(accountsProvider);
                      } catch (error) {
                        messenger.showSnackBar(SnackBar(
                          content: Text('Failed to add offline account: $error'),
                        ));
                      }
                    },
              icon: const Icon(Icons.person_add),
              label: const Text('Add Offline Account'),
            ),
            const SizedBox(height: 24),
            const Text('Instance Management', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            Text('Java Runtime', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 8),
            Column(
              children: runtimes.map((runtime) {
                final groupValue = _selectedRuntimeId ?? (runtimes.isNotEmpty ? runtimes.first.id : null);
                // ignore: deprecated_member_use
                return RadioListTile<String>(
                  // ignore: deprecated_member_use
                  value: runtime.id,
                  // ignore: deprecated_member_use
                  groupValue: groupValue,
                  title: Text(runtime.label),
                  subtitle: Text('${runtime.version} @ ${runtime.path}'),
                  // ignore: deprecated_member_use
                  onChanged: (value) => setState(() => _selectedRuntimeId = value),
                );
              }).toList(),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _runtimeLabelController,
              decoration: const InputDecoration(labelText: 'Runtime label'),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _runtimeVersionController,
              decoration: const InputDecoration(labelText: 'Runtime version'),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _runtimePathController,
              decoration: const InputDecoration(labelText: 'Runtime path'),
            ),
            const SizedBox(height: 8),
            ElevatedButton.icon(
              onPressed: () async {
                final runtime = JavaRuntimeModel(
                  id: DateTime.now().millisecondsSinceEpoch.toString(),
                  label: _runtimeLabelController.text.trim(),
                  version: _runtimeVersionController.text.trim(),
                  path: _runtimePathController.text.trim(),
                );
                await ref.read(javaRuntimeProvider.notifier).addRuntime(runtime);
                setState(() {
                  _selectedRuntimeId = runtime.id;
                });
              },
              icon: const Icon(Icons.storage),
              label: const Text('Add Runtime'),
            ),
            const SizedBox(height: 16),
            versionsAsync.when(
              data: (versions) {
                final selected = _selectedVersionId ?? (_instanceVersionController.text.isNotEmpty
                    ? _instanceVersionController.text
                    : (versions.isNotEmpty ? versions.first.id : null));
                if (_selectedVersionId == null && selected != null) {
                  _selectedVersionId = selected;
                }
                if (versions.isEmpty) {
                  selectedVersion = null;
                } else {
                  selectedVersion = versions.firstWhere(
                    (entry) => entry.id == selected,
                    orElse: () => versions.first,
                  );
                }
                // ignore: deprecated_member_use
                return DropdownButtonFormField<String>(
                  // ignore: deprecated_member_use
                  value: selected,
                  decoration: const InputDecoration(labelText: 'Minecraft version'),
                  items: versions.map((version) => DropdownMenuItem(value: version.id, child: Text(version.id))).toList(),
                  onChanged: (value) {
                    setState(() {
                      _selectedVersionId = value;
                      if (value != null) {
                        _instanceVersionController.text = value;
                      }
                    });
                  },
                );
              },
              loading: () => const SizedBox(
                height: 56,
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (error, stack) => Text('Failed to load versions: $error'),
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              children: ['vanilla', 'forge', 'fabric', 'quilt', 'neoforge']
                  .map((loader) => FilterChip(
                        label: Text(loader.toUpperCase()),
                        selected: loaderSelection.contains(loader),
                        onSelected: (active) {
                          final next = Set<String>.from(loaderSelection);
                          if (active) {
                            next.add(loader);
                          } else {
                            next.remove(loader);
                          }
                          loaderNotifier.state = next.isEmpty ? {'vanilla'} : next;
                        },
                      ))
                  .toList(),
            ),
            const SizedBox(height: 8),
            Text(
              _compatibilityMessage(selectedVersion, loaderSelection),
              style: Theme.of(context).textTheme.bodySmall,
            ),
            const SizedBox(height: 8),
            ElevatedButton.icon(
              onPressed: _instanceNameController.text.trim().isEmpty || _instanceVersionController.text.trim().isEmpty
                  ? null
                  : () async {
                      final messenger = ScaffoldMessenger.of(context);
                      final selectedLoader = loaderSelection.isEmpty ? 'vanilla' : loaderSelection.first;
                      final instance = InstanceModel.create(
                        name: _instanceNameController.text.trim(),
                        version: _instanceVersionController.text.trim(),
                        loader: _mapLoader(selectedLoader),
                      );
                      await instanceNotifier.addInstance(instance);
                      messenger.showSnackBar(SnackBar(
                        content: Text('Created instance ${instance.name}'),
                      ));
                    },
              icon: const Icon(Icons.add_box),
              label: const Text('Create Instance'),
            ),
            const SizedBox(height: 12),
            ...instances.map((instance) => Card(
                  child: ListTile(
                    leading: const Icon(Icons.layers),
                    title: Text(instance.name),
                    subtitle: Text('${instance.version} • ${instance.loader.name.toUpperCase()}'),
                  ),
                )),
            const SizedBox(height: 24),
            const Text('Modrinth Integration', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            TextField(
              controller: _modrinthSearchController,
              decoration: const InputDecoration(
                labelText: 'Search Modrinth projects',
                suffixIcon: Icon(Icons.search),
              ),
              onSubmitted: (value) => modrinthNotifier.search(value),
            ),
            const SizedBox(height: 12),
            modrinthState.when(
              data: (projects) => Column(
                children: projects.map((project) {
                  return Card(
                    child: ListTile(
                      title: Text(project.title),
                      subtitle: Text('${project.categories.join(', ')} • ${project.downloads} downloads'),
                      trailing: IconButton(
                        icon: const Icon(Icons.open_in_new),
                        onPressed: () => launchUrlString('https://modrinth.com/${project.slug}'),
                      ),
                    ),
                  );
                }).toList(),
              ),
              loading: () => const SizedBox(
                height: 48,
                child: Center(child: LinearProgressIndicator()),
              ),
              error: (error, stack) => Text('Modrinth search failed: $error'),
            ),
            const SizedBox(height: 24),
            Wrap(
              spacing: 12,
              runSpacing: 12,
              children: const [
                _FeatureChip(label: 'Profiles'),
                _FeatureChip(label: 'Instances'),
                _FeatureChip(label: 'Modrinth'),
                _FeatureChip(label: 'Diagnostics'),
              ],
            ),
          ],
        ),
      ),
    );
  }

  String _formatTimestamp(DateTime timestamp) {
    final h = timestamp.hour.toString().padLeft(2, '0');
    final m = timestamp.minute.toString().padLeft(2, '0');
    final s = timestamp.second.toString().padLeft(2, '0');
    return '$h:$m:$s';
  }
}

class _AccountTile extends ConsumerWidget {
  const _AccountTile(this.account, {required this.isActive});

  final AccountModel account;
  final bool isActive;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Card(
      child: ListTile(
        leading: const Icon(Icons.person),
        title: Text(account.username),
        subtitle: Text(account.provider),
        trailing: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            IconButton(
              icon: const Icon(Icons.refresh),
              tooltip: 'Refresh tokens',
              onPressed: () async {
                final messenger = ScaffoldMessenger.of(context);
                try {
                  final repo = ref.read(authRepositoryProvider);
                  final refreshed = await repo.refreshToken(account);
                  messenger.showSnackBar(SnackBar(
                    content: Text('Token refreshed (${refreshed.accessToken.substring(0, 8)})...'),
                  ));
                } catch (error) {
                  messenger.showSnackBar(SnackBar(
                    content: Text('Token refresh failed: $error'),
                  ));
                }
              },
            ),
            IconButton(
              icon: Icon(isActive ? Icons.radio_button_checked : Icons.radio_button_unchecked),
              tooltip: isActive ? 'Selected account' : 'Switch to this account',
              onPressed: () {
                ref.read(activeAccountProvider.notifier).select(account);
                ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                  content: Text('${account.username} is now active'),
                ));
              },
            ),
          ],
        ),
        tileColor: isActive ? Theme.of(context).colorScheme.primary.withAlpha(26) : null,
      ),
    );
  }
}

class _DeviceCodeCard extends StatelessWidget {
  const _DeviceCodeCard({required this.code});

  final MicrosoftDeviceCode code;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Code: ${code.userCode}', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('Expires in ${code.expiresIn}s', style: Theme.of(context).textTheme.bodySmall),
            const SizedBox(height: 4),
            Text(code.message),
            const SizedBox(height: 8),
            TextButton.icon(
              onPressed: () => launchUrlString(code.verificationUri),
              icon: const Icon(Icons.open_in_new),
              label: const Text('Open Verification URL'),
            ),
          ],
        ),
      ),
    );
  }
}

class _FeatureChip extends StatelessWidget {
  const _FeatureChip({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) => Chip(label: Text(label));
}

InstanceLoader _mapLoader(String loader) {
  return InstanceLoader.values.firstWhere(
    (entry) => entry.name == loader,
    orElse: () => InstanceLoader.vanilla,
  );
}

String _compatibilityMessage(VersionDescriptor? version, Set<String> loaders) {
  if (version == null) return 'Version info loading...';
  final incompatible = loaders.where((loader) => loader != 'vanilla' && version.type == 'snapshot').toList();
  if (incompatible.isEmpty) {
    return 'Selected loaders compatible with ${version.id} (${version.type})';
  }
  return 'Snapshots may not support: ${incompatible.join(', ')}';
}
