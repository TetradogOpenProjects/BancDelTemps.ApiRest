<?php

namespace Database\Factories;

use App\Models\Message;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

class MessageFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = Message::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'from_id'=>User::factory(),
            'to_id'=>User::factory(),
            'content'=>$this->faker->sentence(),
            'fromHidden'=>$this->faker->boolean(),
            'toHidden'=>$this->faker->boolean(),
            'toCheck'=>$this->faker->boolean()
        ];
    }
}
