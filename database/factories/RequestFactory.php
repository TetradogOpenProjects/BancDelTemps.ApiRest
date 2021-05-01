<?php

namespace Database\Factories;

use App\Models\Request;
use Illuminate\Database\Eloquent\Factories\Factory;

class RequestFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = Request::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'user_id'=>User::factory(),
            'approvedBy_id'=>User::factory(),
            'location_id'=>Location::factory(),
            'title'=>$this->faker->paragraph,
            'description'=>$this->faker->paragraph,
            'numUsersRequired'=>$this->faker->random_int(1,10)
        ];
    }
}
