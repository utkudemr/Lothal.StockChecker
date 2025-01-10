<h3>Overview</h3>
This project demonstrates how to handle race conditions in a stock management system using Redis and Lua scripts. By leveraging Redis for data storage and Lua scripts for atomic operations, we can prevent multiple requests from simultaneously modifying the same data, ensuring consistency and avoiding negative stock values.

<h3>Key Concepts</h3>
<ul>
  <li>
Race Condition: A situation where multiple processes access shared data and attempt to modify it simultaneously, which can lead to inconsistency or errors.</li>
  <li>Lua Scripts in Redis: Lua scripts allow us to perform multiple Redis commands atomically in a single transaction. This helps ensure that operations like stock updates happen in a consistent manner without interference from other processes.</li>
</ul>
